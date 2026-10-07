using MediatR;
using OrderPulse.Application.Common.DTOs;
using OrderPulse.Domain.Entities;
using OrderPulse.Domain.Exceptions;
using OrderPulse.Domain.Repositories;
using OrderPulse.Domain.ValueObjects;

namespace OrderPulse.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Command Handler for creating an order.
/// In Laravel: Corresponds to an Action class (e.g. CreateOrderAction.php).
/// Coordinates domain entities, repositories, and the Unit of Work.
/// </summary>
public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Customer exists
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Customer), request.CustomerId);

        // 2. Fetch all requested products in one batch query
        var productIds = request.Items.Select(i => i.ProductId).Distinct();
        var products = (await _productRepository.GetByIdsAsync(productIds, cancellationToken))
            .ToDictionary(p => p.Id);

        // 3. Create Shipping Address Value Object
        var shippingAddress = new Address(
            request.Street,
            request.City,
            request.State,
            request.PostalCode,
            request.Country
        );

        // 4. Create Order Aggregate
        var order = Order.Create(Guid.NewGuid(), customer.Id, shippingAddress);

        // 5. Reserve inventory and add items
        foreach (var itemRequest in request.Items)
        {
            if (!products.TryGetValue(itemRequest.ProductId, out var product))
            {
                throw new EntityNotFoundException(nameof(Product), itemRequest.ProductId);
            }

            // Encapsulated domain logic: checks availability, deducts available, increases reserved
            product.ReserveStock(itemRequest.Quantity);

            order.AddItem(product.Id, product.Name, product.Price, itemRequest.Quantity);
        }

        // 6. Submit the order (raises OrderCreatedDomainEvent)
        order.Submit();

        // 7. Persist changes atomically
        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Return projected DTO
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
