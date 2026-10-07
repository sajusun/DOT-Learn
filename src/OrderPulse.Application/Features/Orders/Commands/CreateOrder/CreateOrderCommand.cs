using MediatR;
using OrderPulse.Application.Common.DTOs;

namespace OrderPulse.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);

public sealed record CreateOrderCommand(
    Guid CustomerId,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    List<CreateOrderItemRequest> Items
) : IRequest<OrderDto>;
