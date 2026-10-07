using OrderPulse.Domain.Entities;
using OrderPulse.Domain.Exceptions;
using OrderPulse.Domain.ValueObjects;
using Xunit;

namespace OrderPulse.Domain.UnitTests;

public class ProductTests
{
    [Fact]
    public void ReserveStock_WhenSufficientStock_ShouldDeductAvailableAndIncreaseReserved()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "SKU-100",
            "Gaming Mouse",
            "Wireless Ergonomic Gaming Mouse",
            new Money(79.99m, "USD"),
            initialStock: 10
        );

        // Act
        product.ReserveStock(3);

        // Assert
        Assert.Equal(7, product.AvailableStock);
        Assert.Equal(3, product.ReservedStock);
    }

    [Fact]
    public void ReserveStock_WhenInsufficientStock_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "SKU-100",
            "Gaming Mouse",
            "Wireless Ergonomic Gaming Mouse",
            new Money(79.99m, "USD"),
            initialStock: 5
        );

        // Act & Assert
        var ex = Assert.Throws<InsufficientStockException>(() => product.ReserveStock(10));
        Assert.Equal(10, ex.RequestedQuantity);
        Assert.Equal(5, ex.AvailableQuantity);
    }

    [Fact]
    public void ReleaseStock_ShouldReturnReservedStockToAvailable()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "SKU-100",
            "Gaming Mouse",
            "Wireless Ergonomic Gaming Mouse",
            new Money(79.99m, "USD"),
            initialStock: 10
        );
        product.ReserveStock(4);

        // Act
        product.ReleaseStock(2);

        // Assert
        Assert.Equal(8, product.AvailableStock);
        Assert.Equal(2, product.ReservedStock);
    }
}
