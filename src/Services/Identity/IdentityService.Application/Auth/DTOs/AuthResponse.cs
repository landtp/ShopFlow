namespace IdentityService.Application.Auth.DTOs;

// Record dùng cho tất cả auth responses — nhất quán
public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt
);