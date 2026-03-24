using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Repositories;

internal sealed class UserRepository(IdentityDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(
       Guid id, CancellationToken ct = default) =>
       await context.Users
           .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(
        string email, CancellationToken ct = default) =>
        await context.Users
            .FirstOrDefaultAsync(
                u => u.Email == Domain.ValueObjects.Email.Create(email), ct);

    public async Task<bool> ExistsByEmailAsync(
        string email, CancellationToken ct = default) =>
        await context.Users
            .AnyAsync(
                u => u.Email == Domain.ValueObjects.Email.Create(email), ct);

    public async Task AddAsync(
        User user, CancellationToken ct = default) =>
        await context.Users.AddAsync(user, ct);

    public async Task UpdateAsync(
        User user, CancellationToken ct = default)
    {
        context.Users.Update(user);
        await Task.CompletedTask;
    }
}
