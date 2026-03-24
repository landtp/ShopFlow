using IdentityService.Application.Abstractions;
using IdentityService.Application.Auth.DTOs;
using IdentityService.Application.Exceptions;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Auth.Commands.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(
        RefreshTokenCommand command,
        CancellationToken ct)
    {
        var user = await userRepository
            .GetByIdAsync(command.UserId, ct);

        if (user is null || !user.IsActive)
            throw new UnauthorizedException("Token không hợp lệ");

        // User entity tự kiểm tra token hợp lệ
        if (!user.IsRefreshTokenValid(command.RefreshToken))
            throw new UnauthorizedException(
                "Refresh token hết hạn hoặc không hợp lệ");

        // Rotate refresh token — mỗi lần dùng phải đổi token mới
        // Phòng token reuse attack
        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.SetRefreshToken(
            newRefreshToken,
            DateTime.UtcNow.AddDays(7));

        await userRepository.UpdateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Role.Value,
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(15));
    }
}
