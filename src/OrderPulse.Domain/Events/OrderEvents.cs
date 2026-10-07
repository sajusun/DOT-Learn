using OrderPulse.Domain.Common;

namespace OrderPulse.Domain.Events;

public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTime OccurredOnUtc
) : IDomainEvent
{
    public OrderCreatedDomainEvent(Guid orderId, Guid customerId, decimal totalAmount, string currency)
        : this(orderId, customerId, totalAmount, currency, DateTime.UtcNow)
    {
    }
}

public sealed record OrderCancelledDomainEvent(
    Guid OrderId,
    string Reason,
    DateTime OccurredOnUtc
) : IDomainEvent
{
    public OrderCancelledDomainEvent(Guid orderId, string reason)
        : this(orderId, reason, DateTime.UtcNow)
    {
    }
}

public sealed record OrderPaidDomainEvent(
    Guid OrderId,
    decimal Amount,
    string TransactionId,
    DateTime OccurredOnUtc
) : IDomainEvent
{
    public OrderPaidDomainEvent(Guid orderId, decimal amount, string transactionId)
        : this(orderId, amount, transactionId, DateTime.UtcNow)
    {
    }
}
