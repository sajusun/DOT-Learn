using FluentValidation;
using MediatR;
using OrderPulse.Domain.Entities;
using OrderPulse.Domain.Exceptions;
using OrderPulse.Domain.Repositories;

namespace OrderPulse.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string Reason) : IRequest;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500).WithMessage("Cancellation reason is required.");
    }
}

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Order), request.OrderId);

        // Fetch products to release reserved stock
        var productIds = order.Items.Select(i => i.ProductId).Distinct();
        var products = (await _productRepository.GetByIdsAsync(productIds, cancellationToken))
            .ToDictionary(p => p.Id);

        foreach (var item in order.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
            {
                product.ReleaseStock(item.Quantity);
            }
        }

        // Domain state change and event raise
        order.Cancel(request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
