using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtTokenService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork uow, IJwtTokenService jwtService, ILogger<AuthService> logger)
    {
        _uow = uow;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _uow.Users.UsernameExistsAsync(request.Username, ct))
            throw new Domain.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                { "Username", new[] { "Username is already taken." } }
            });

        var user = new ApplicationUser
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("New user registered: {Username}", user.Username);
        return await IssueTokensAsync(user, ct);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByUsernameAsync(request.Username, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid username or password.");

        _logger.LogInformation("User logged in: {Username}", user.Username);
        return await IssueTokensAsync(user, ct);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var principal = _jwtService.ValidateExpiredToken(request.AccessToken);
        if (principal is null)
            throw new UnauthorizedException("Invalid access token.");

        var refreshToken = await _uow.Users.GetRefreshTokenAsync(request.RefreshToken, ct);

        if (refreshToken is null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // Rotate refresh token
        await _uow.Users.RevokeRefreshTokenAsync(refreshToken, ct);

        var newRefresh = _jwtService.GenerateRefreshToken(refreshToken.UserId);
        await _uow.Users.AddRefreshTokenAsync(newRefresh, ct);
        await _uow.SaveChangesAsync(ct);

        var accessToken = _jwtService.GenerateAccessToken(refreshToken.User);

        _logger.LogInformation("Token refreshed for user ID {UserId}", refreshToken.UserId);
        return new TokenResponse(accessToken, newRefresh.Token, newRefresh.ExpiresAt, refreshToken.User.Username, refreshToken.User.Role);
    }

    public async Task RevokeTokenAsync(RevokeTokenRequest request, CancellationToken ct = default)
    {
        var refreshToken = await _uow.Users.GetRefreshTokenAsync(request.RefreshToken, ct)
            ?? throw new UnauthorizedException("Refresh token not found.");

        await _uow.Users.RevokeRefreshTokenAsync(refreshToken, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Refresh token revoked for user ID {UserId}", refreshToken.UserId);
    }

    private async Task<TokenResponse> IssueTokensAsync(ApplicationUser user, CancellationToken ct)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id);
        await _uow.Users.AddRefreshTokenAsync(refreshToken, ct);
        await _uow.SaveChangesAsync(ct);
        return new TokenResponse(accessToken, refreshToken.Token, refreshToken.ExpiresAt, user.Username, user.Role);
    }
}
