using MediatR;
using OrderPulse.Application.Common.DTOs;
using OrderPulse.Domain.Repositories;

namespace OrderPulse.Application.Features.Orders.Queries.GetCustomerOrders;

public sealed record GetCustomerOrdersQuery(Guid CustomerId) : IRequest<IReadOnlyList<OrderDto>>;

public sealed class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetCustomerOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        return orders.Select(order => new OrderDto(
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
        )).ToList();
    }
}
