using IdentityService.Application.Abstractions;
using IdentityService.Domain.Repositories;
using MediatR;

namespace IdentityService.Application.Auth.Commands.Logout;

internal sealed class LogoutCommandHandler(
    IUserRepository userRepository,
    ITokenBlacklistService blacklistService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Unit> Handle(
        LogoutCommand command, CancellationToken ct)
    {
        var user = await userRepository
            .GetByIdAsync(command.UserId, ct);

        if (user is not null)
        {
            // Revoke refresh token trong DB
            user.RevokeRefreshToken();
            await userRepository.UpdateAsync(user, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }

        // Blacklist access token trong Redis
        // Token còn 15 phút → blacklist đúng 15 phút
        await blacklistService.BlacklistAsync(
            command.AccessToken,
            TimeSpan.FromMinutes(15), ct);

        return Unit.Value;
    }
}