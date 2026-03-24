using IdentityService.Application.Abstractions;
using IdentityService.Application.Auth.DTOs;

namespace IdentityService.Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    Guid UserId,
    string RefreshToken
) : ICommand<AuthResponse>;
