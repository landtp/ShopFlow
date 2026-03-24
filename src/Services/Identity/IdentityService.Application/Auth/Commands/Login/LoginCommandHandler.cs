using IdentityService.Application.Abstractions;
using IdentityService.Application.Auth.DTOs;
using IdentityService.Application.Exceptions;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Auth.Commands.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(
        LoginCommand command,
        CancellationToken ct)
    {
        // Tìm user theo email
        var user = await userRepository
            .GetByEmailAsync(command.Email, ct);

        // Dùng cùng 1 error message cho cả 2 trường hợp
        // → tránh username enumeration attack
        if (user is null || !user.IsActive)
            throw new UnauthorizedException(
                "Email hoặc password không đúng");

        // Verify password với BCrypt
        var passwordValid = passwordHasher
            .Verify(command.Password, user.PasswordHash);

        if (!passwordValid)
            throw new UnauthorizedException(
                "Email hoặc password không đúng");

        // Generate tokens mới
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.SetRefreshToken(
            refreshToken,
            DateTime.UtcNow.AddDays(7));

        await userRepository.UpdateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Role.Value,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15));
    }
}
