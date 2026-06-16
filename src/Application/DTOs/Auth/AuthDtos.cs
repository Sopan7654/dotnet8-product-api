namespace Application.DTOs.Auth;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(string Username, string Email, string Password);

public record RefreshTokenRequest(string AccessToken, string RefreshToken);

public record RevokeTokenRequest(string RefreshToken);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Username,
    string Role
);
