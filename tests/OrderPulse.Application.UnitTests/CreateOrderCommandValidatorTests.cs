using OrderPulse.Application.Features.Orders.Commands.CreateOrder;
using Xunit;

namespace OrderPulse.Application.UnitTests;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldHaveNoErrors()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            "Main St",
            "Metropolis",
            "NY",
            "10001",
            "USA",
            [new CreateOrderItemRequest(Guid.NewGuid(), 2)]
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenCustomerIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.Empty,
            "Main St",
            "Metropolis",
            "NY",
            "10001",
            "USA",
            [new CreateOrderItemRequest(Guid.NewGuid(), 2)]
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOrderCommand.CustomerId));
    }

    [Fact]
    public void Validate_WhenItemsListIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            "Main St",
            "Metropolis",
            "NY",
            "10001",
            "USA",
            []
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOrderCommand.Items));
    }
}
