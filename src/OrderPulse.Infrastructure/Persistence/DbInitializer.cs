using Microsoft.EntityFrameworkCore;
using OrderPulse.Domain.Entities;
using OrderPulse.Domain.ValueObjects;

namespace OrderPulse.Infrastructure.Persistence;

/// <summary>
/// Database Initializer and Seeder.
/// In Laravel: Like DatabaseSeeder.php and Model Factories (php artisan db:seed).
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Apply pending migrations automatically on startup
        await context.Database.MigrateAsync();

        // Check if data already exists
        if (await context.Products.AnyAsync())
        {
            return; // DB has already been seeded
        }

        // 1. Seed Customer
        var customerAddress = new Address("456 Enterprise Blvd", "Seattle", "WA", "98101", "USA");
        var customer = new Customer(
            id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            email: "alex.engineer@enterprise.com",
            firstName: "Alex",
            lastName: "Vance",
            defaultShippingAddress: customerAddress
        );

        await context.Customers.AddAsync(customer);

        // 2. Seed Products
        var products = new List<Product>
        {
            new(
                id: Guid.Parse("22222222-2222-2222-2222-222222222221"),
                sku: "KB-MECH-01",
                name: "UltraKey RGB Mechanical Keyboard",
                description: "Wireless mechanical keyboard with hot-swappable tactile switches.",
                price: new Money(149.99m, "USD"),
                initialStock: 50
            ),
            new(
                id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
                sku: "MO-PRO-02",
                name: "PrecisionPro Ergonomic Mouse",
                description: "High-precision wireless ergonomic mouse with hyper-scroll wheel.",
                price: new Money(89.50m, "USD"),
                initialStock: 75
            ),
            new(
                id: Guid.Parse("22222222-2222-2222-2222-222222222223"),
                sku: "MON-4K-03",
                name: "UltraWide 34\" Curved 4K Monitor",
                description: "IPS 144Hz HDR curved gaming and productivity monitor.",
                price: new Money(699.00m, "USD"),
                initialStock: 15
            )
        };

        await context.Products.AddRangeAsync(products);

        await context.SaveChangesAsync();
    }
}
