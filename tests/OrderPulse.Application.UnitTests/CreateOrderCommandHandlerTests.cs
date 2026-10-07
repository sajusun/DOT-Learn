using NSubstitute;
using OrderPulse.Application.Features.Orders.Commands.CreateOrder;
using OrderPulse.Domain.Entities;
using OrderPulse.Domain.Exceptions;
using OrderPulse.Domain.Repositories;
using OrderPulse.Domain.ValueObjects;
using Xunit;

namespace OrderPulse.Application.UnitTests;

public class CreateOrderCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(
            _customerRepository,
            _productRepository,
            _orderRepository,
            _unitOfWork
        );
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReserveStockSaveOrderAndReturnDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer(customerId, "test@example.com", "John", "Doe");
        _customerRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(customer);

        var productId = Guid.NewGuid();
        var product = new Product(productId, "SKU-01", "Keyboard", "Mechanical", new Money(100m, "USD"), 10);
        _productRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product]);

        var command = new CreateOrderCommand(
            customerId,
            "123 Street",
            "City",
            "State",
            "12345",
            "Country",
            [new CreateOrderItemRequest(productId, 2)]
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(200m, result.TotalAmount);
        Assert.Equal(8, product.AvailableStock); // 10 - 2 = 8
        Assert.Equal(2, product.ReservedStock);

        await _orderRepository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _customerRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var command = new CreateOrderCommand(
            customerId, "Street", "City", "State", "12345", "Country",
            [new CreateOrderItemRequest(Guid.NewGuid(), 1)]
        );

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
