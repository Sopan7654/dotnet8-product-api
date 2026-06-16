using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IJwtTokenService> _jwtMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_uowMock.Object, _jwtMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokenResponse_WithValidCredentials()
    {
        // Arrange
        var hash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        var user = new ApplicationUser { Id = 1, Username = "admin", Email = "admin@test.com", PasswordHash = hash, Role = "Admin" };
        var refreshToken = new RefreshToken { Token = "rt_token", ExpiresAt = DateTime.UtcNow.AddDays(7), UserId = 1 };

        _uowMock.Setup(u => u.Users.GetByUsernameAsync("admin", default)).ReturnsAsync(user);
        _jwtMock.Setup(j => j.GenerateAccessToken(user)).Returns("access_token");
        _jwtMock.Setup(j => j.GenerateRefreshToken(1)).Returns(refreshToken);
        _uowMock.Setup(u => u.Users.AddRefreshTokenAsync(refreshToken, default)).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.LoginAsync(new LoginRequest("admin", "Admin@123"));

        // Assert
        Assert.Equal("access_token", result.AccessToken);
        Assert.Equal("rt_token", result.RefreshToken);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WithBadCredentials()
    {
        _uowMock.Setup(u => u.Users.GetByUsernameAsync("bad", default)).ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest("bad", "wrongpass")));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsValidationException_WhenUsernameTaken()
    {
        _uowMock.Setup(u => u.Users.UsernameExistsAsync("admin", default)).ReturnsAsync(true);

        await Assert.ThrowsAsync<Domain.Exceptions.ValidationException>(() =>
            _service.RegisterAsync(new RegisterRequest("admin", "admin@test.com", "Admin@123")));
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WithHashedPassword()
    {
        var request = new RegisterRequest("newuser", "newuser@test.com", "Pass@word1");
        var refreshToken = new RefreshToken { Token = "rt", ExpiresAt = DateTime.UtcNow.AddDays(7), UserId = 2 };

        _uowMock.Setup(u => u.Users.UsernameExistsAsync("newuser", default)).ReturnsAsync(false);
        _uowMock.Setup(u => u.Users.AddAsync(It.IsAny<ApplicationUser>(), default))
            .ReturnsAsync((ApplicationUser u, CancellationToken _) => { u.Id = 2; return u; });
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _jwtMock.Setup(j => j.GenerateAccessToken(It.IsAny<ApplicationUser>())).Returns("at");
        _jwtMock.Setup(j => j.GenerateRefreshToken(It.IsAny<int>())).Returns(refreshToken);
        _uowMock.Setup(u => u.Users.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), default)).Returns(Task.CompletedTask);

        var result = await _service.RegisterAsync(request);

        Assert.Equal("at", result.AccessToken);
        _uowMock.Verify(u => u.Users.AddAsync(It.IsAny<ApplicationUser>(), default), Times.Once);
    }
}
