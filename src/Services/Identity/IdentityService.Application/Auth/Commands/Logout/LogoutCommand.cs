using IdentityService.Application.Abstractions;

namespace IdentityService.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(
    Guid UserId,
    string AccessToken
) : ICommand;
