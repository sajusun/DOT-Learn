using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPulse.Domain.Entities;

namespace OrderPulse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for Customer.
/// In Laravel: Like schema definitions in database/migrations/..._create_customers_table.php
/// </summary>
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAtUtc)
            .IsRequired();

        // Owned Value Object (maps properties into the same customers table)
        builder.OwnsOne(c => c.DefaultShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("shipping_street").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("shipping_city").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("shipping_state").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("shipping_postal_code").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("shipping_country").HasMaxLength(100);
        });
    }
}
