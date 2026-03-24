using IdentityService.Application.Abstractions;
using IdentityService.Application.Auth.DTOs;

namespace IdentityService.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : ICommand<AuthResponse>;
