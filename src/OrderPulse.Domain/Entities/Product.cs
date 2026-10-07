using OrderPulse.Domain.Common;
using OrderPulse.Domain.Exceptions;
using OrderPulse.Domain.ValueObjects;

namespace OrderPulse.Domain.Entities;

public sealed class Product : AggregateRoot<Guid>
{
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Price { get; private set; } = null!;
    public int AvailableStock { get; private set; }
    public int ReservedStock { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Concurrency token (PostgreSQL xmin or version timestamp)
    public uint Version { get; private set; }

    private Product() { }

    public Product(Guid id, string sku, string name, string description, Money price, int initialStock)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU is required.", nameof(sku));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name is required.", nameof(name));
        if (initialStock < 0) throw new ArgumentException("Initial stock cannot be negative.", nameof(initialStock));

        Sku = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = description.Trim();
        Price = price ?? throw new ArgumentNullException(nameof(price));
        AvailableStock = initialStock;
        ReservedStock = 0;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to reserve must be greater than zero.", nameof(quantity));

        if (AvailableStock < quantity)
            throw new InsufficientStockException(Id, quantity, AvailableStock);

        AvailableStock -= quantity;
        ReservedStock += quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ReleaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to release must be greater than zero.", nameof(quantity));

        if (ReservedStock < quantity)
            throw new InvalidOperationException($"Cannot release {quantity} units; only {ReservedStock} units are reserved.");

        ReservedStock -= quantity;
        AvailableStock += quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Restock quantity must be positive.", nameof(quantity));

        AvailableStock += quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description, Money price)
    {
        Name = name.Trim();
        Description = description.Trim();
        Price = price ?? throw new ArgumentNullException(nameof(price));
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
