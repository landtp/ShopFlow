using IdentityService.Domain.Entities;

namespace IdentityService.Application.Abstractions;

// Interface ở Application — implementation ở Infrastructure
// Đây là ISP (Interface Segregation) — tách riêng token concerns
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? GetUserIdFromToken(string accessToken);
}

