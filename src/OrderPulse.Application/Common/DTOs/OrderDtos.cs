namespace OrderPulse.Application.Common.DTOs;

/// <summary>
/// DTO representing an Order.
/// In Laravel: Like OrderResource::toArray($request).
/// </summary>
public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    string PaymentStatus,
    decimal TotalAmount,
    string Currency,
    string ShippingAddress,
    string? CancellationReason,
    string? PaymentTransactionId,
    DateTime CreatedAtUtc,
    IReadOnlyList<OrderItemDto> Items
);

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal TotalPrice
);

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int AvailableStock,
    bool IsActive
);
