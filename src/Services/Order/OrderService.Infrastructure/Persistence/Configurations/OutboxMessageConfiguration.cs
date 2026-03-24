using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Outbox;

namespace OrderService.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Payload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.ProcessedAt);
        builder.Property(o => o.Error).HasMaxLength(2000);

        // Index để OutboxProcessor query nhanh
        builder.HasIndex(o => o.ProcessedAt);
    }
}