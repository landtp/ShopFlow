using IdentityService.Application.Abstractions;
using IdentityService.Application.Auth.DTOs;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Application.Exceptions;

namespace IdentityService.Application.Auth.Commands.Register;

internal sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(
        RegisterCommand command,
        CancellationToken ct)
    {
        // Kiểm tra email đã tồn tại chưa
        var emailExists = await userRepository
            .ExistsByEmailAsync(command.Email, ct);

        if (emailExists)
            throw new ConflictException(
                $"Email '{command.Email}' đã được đăng ký");

        // Hash password — không bao giờ lưu plain text
        var passwordHash = passwordHasher.Hash(command.Password);

        // Tạo User qua factory method — Domain validate
        var user = User.Create(
            command.Email,
            passwordHash,
            command.FirstName,
            command.LastName);

        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        // Lưu refresh token vào User
        user.SetRefreshToken(
            refreshToken,
            DateTime.UtcNow.AddDays(7));

        await userRepository.AddAsync(user, ct);
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
