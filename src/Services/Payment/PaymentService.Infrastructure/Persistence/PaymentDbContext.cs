using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence;

public sealed class PaymentDbContext(
    DbContextOptions<PaymentDbContext> options)
    : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(b =>
        {
            b.ToTable("Payments");
            b.HasKey(p => p.Id);
            b.Property(p => p.OrderId).IsRequired();
            b.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            b.Property(p => p.Status).HasMaxLength(20).IsRequired();
            // Unique index — không tạo 2 payment cho 1 order
            b.HasIndex(p => p.OrderId).IsUnique();
        });

        modelBuilder.Entity<ProcessedMessage>(b =>
        {
            b.ToTable("ProcessedMessages");
            b.HasKey(p => p.MessageId);
            b.Property(p => p.EventType).HasMaxLength(100);
        });
    }
}