using IdentityService.Application.Abstractions;
using IdentityService.Application.Auth.DTOs;

namespace IdentityService.Application.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : ICommand<AuthResponse>;
