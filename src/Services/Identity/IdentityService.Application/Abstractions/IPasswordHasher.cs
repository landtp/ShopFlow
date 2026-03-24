namespace IdentityService.Application.Abstractions;

// Tách riêng password hashing — dễ đổi từ BCrypt sang Argon2 sau này
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
