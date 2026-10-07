using OrderPulse.Domain.Entities;
using OrderPulse.Domain.Enums;
using OrderPulse.Domain.Events;
using OrderPulse.Domain.Exceptions;
using OrderPulse.Domain.ValueObjects;
using Xunit;

namespace OrderPulse.Domain.UnitTests;

public class OrderTests
{
    private readonly Address _validAddress = new("123 Main St", "Metropolis", "NY", "10001", "USA");

    [Fact]
    public void CreateOrder_ShouldInitializeWithDraftStatusAndEmptyItems()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var order = Order.Create(Guid.NewGuid(), customerId, _validAddress);

        // Assert
        Assert.Equal(OrderStatus.Draft, order.Status);
        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
        Assert.Empty(order.Items);
        Assert.Equal(0, order.TotalAmount.Amount);
    }

    [Fact]
    public void AddItem_ShouldAddLinesAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _validAddress);
        var unitPrice = new Money(50.00m, "USD");

        // Act
        order.AddItem(Guid.NewGuid(), "Mechanical Keyboard", unitPrice, 2);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(100.00m, order.TotalAmount.Amount);
    }

    [Fact]
    public void Submit_WhenItemsPresent_ShouldTransitionToSubmittedAndRaiseDomainEvent()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _validAddress);
        order.AddItem(Guid.NewGuid(), "USB-C Cable", new Money(15.00m, "USD"), 1);

        // Act
        order.Submit();

        // Assert
        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderCreatedDomainEvent);
    }

    [Fact]
    public void Submit_WhenOrderIsEmpty_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _validAddress);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Submit());
    }

    [Fact]
    public void Cancel_WhenValidState_ShouldTransitionToCancelledAndRaiseEvent()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _validAddress);
        order.AddItem(Guid.NewGuid(), "Desk Mat", new Money(25.00m, "USD"), 1);

        // Act
        order.Cancel("Customer requested cancellation");

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("Customer requested cancellation", order.CancellationReason);
        Assert.Contains(order.DomainEvents, e => e is OrderCancelledDomainEvent);
    }
}
