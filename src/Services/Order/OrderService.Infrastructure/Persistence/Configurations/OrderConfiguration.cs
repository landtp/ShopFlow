using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Aggregates;
using OrderService.Domain.ValueObjects;

namespace OrderService.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Version)
            .IsRowVersion();      
        // IsRowVersion() tự động set IsConcurrencyToken()
        // và ValueGeneratedOnAddOrUpdate()
        // SQL Server tự generate — không cần insert thủ công

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.ShippingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        // Map OrderStatus (Value Object) → string column
        builder.Property(o => o.Status)
            .HasConversion(
                status => status.Value,           // save: OrderStatus → string
                value => OrderStatus.From(value)) // load: string → OrderStatus
            .HasMaxLength(20)
            .IsRequired();

        // Map Money (Value Object) → 2 columns
        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Navigation: Order → OrderItems (1 to many)
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // EF dùng backing field _items thay vì property Items
        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

