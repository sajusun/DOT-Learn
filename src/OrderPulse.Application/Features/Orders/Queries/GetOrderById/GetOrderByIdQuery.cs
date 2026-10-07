using MediatR;
using OrderPulse.Application.Common.DTOs;
using OrderPulse.Domain.Entities;
using OrderPulse.Domain.Exceptions;
using OrderPulse.Domain.Repositories;

namespace OrderPulse.Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Order), request.OrderId);

        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.ShippingAddress.ToString(),
            order.CancellationReason,
            order.PaymentTransactionId,
            order.CreatedAtUtc,
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice.Amount,
                i.UnitPrice.Currency,
                i.Quantity,
                i.TotalPrice.Amount
            )).ToList()
        );
    }
}
