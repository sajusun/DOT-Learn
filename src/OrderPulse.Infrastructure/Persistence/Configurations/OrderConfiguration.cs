using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPulse.Domain.Entities;

namespace OrderPulse.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.HasIndex(o => o.CustomerId);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        builder.Property(o => o.PaymentTransactionId)
            .HasMaxLength(100);

        builder.Property(o => o.CreatedAtUtc)
            .IsRequired();

        // Owned Value Object for Shipping Address
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("shipping_street").HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasColumnName("shipping_city").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("shipping_state").HasMaxLength(100).IsRequired();
            address.Property(a => a.PostalCode).HasColumnName("shipping_postal_code").HasMaxLength(20).IsRequired();
            address.Property(a => a.Country).HasColumnName("shipping_country").HasMaxLength(100).IsRequired();
        });

        // Relationship: Order has many OrderItems
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties and domain events
        builder.Ignore(o => o.TotalAmount);
        builder.Ignore(o => o.DomainEvents);
    }
}
