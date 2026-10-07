using OrderPulse.Domain.Common;
using OrderPulse.Domain.Enums;
using OrderPulse.Domain.Events;
using OrderPulse.Domain.Exceptions;
using OrderPulse.Domain.ValueObjects;

namespace OrderPulse.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _orderItems = [];

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public string? CancellationReason { get; private set; }
    public string? PaymentTransactionId { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _orderItems.AsReadOnly();

    public Money TotalAmount
    {
        get
        {
            if (_orderItems.Count == 0) return Money.Zero();
            var currency = _orderItems[0].UnitPrice.Currency;
            var total = _orderItems.Sum(item => item.TotalPrice.Amount);
            return new Money(total, currency);
        }
    }

    private Order() { } // EF Core required

    public static Order Create(Guid id, Guid customerId, Address shippingAddress)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress)),
            Status = OrderStatus.Draft,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        return order;
    }

    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Cannot add items when order is in '{Status}' state.");

        var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = new OrderItem(Guid.NewGuid(), Id, productId, productName, unitPrice, quantity);
            _orderItems.Add(newItem);
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Cannot remove items when order is in '{Status}' state.");

        var item = _orderItems.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _orderItems.Remove(item);
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOrderStateTransitionException(Status.ToString(), OrderStatus.Submitted.ToString());

        if (_orderItems.Count == 0)
            throw new InvalidOperationException("Cannot submit an empty order.");

        Status = OrderStatus.Submitted;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderCreatedDomainEvent(Id, CustomerId, TotalAmount.Amount, TotalAmount.Currency));
    }

    public void MarkAsPaid(string transactionId)
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOrderStateTransitionException(Status.ToString(), OrderStatus.Paid.ToString());

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID is required.", nameof(transactionId));

        Status = OrderStatus.Paid;
        PaymentStatus = PaymentStatus.Captured;
        PaymentTransactionId = transactionId.Trim();
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderPaidDomainEvent(Id, TotalAmount.Amount, transactionId));
    }

    public void Cancel(string reason)
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Fulfilled)
            throw new InvalidOrderStateTransitionException(Status.ToString(), OrderStatus.Cancelled.ToString());

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason must be provided.", nameof(reason));

        Status = OrderStatus.Cancelled;
        CancellationReason = reason.Trim();
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, reason));
    }
}
