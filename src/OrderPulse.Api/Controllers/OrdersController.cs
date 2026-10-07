using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderPulse.Application.Common.DTOs;
using OrderPulse.Application.Features.Orders.Commands.CancelOrder;
using OrderPulse.Application.Features.Orders.Commands.CreateOrder;
using OrderPulse.Application.Features.Orders.Queries.GetCustomerOrders;
using OrderPulse.Application.Features.Orders.Queries.GetOrderById;

namespace OrderPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Enforces JWT Bearer authentication (mirroring Laravel Sanctum middleware)
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Place a new order with stock reservation.
    /// In Laravel: Like Route::post('/orders', [OrderController::class, 'store'])
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieve order details by unique order identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// List all orders for a specific customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerOrders(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerOrdersQuery(customerId), cancellationToken);
        return Ok(result);
    }

    public sealed record CancelOrderRequestBody(string Reason);

    /// <summary>
    /// Cancel an order and release reserved stock back into inventory.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequestBody request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelOrderCommand(id, request.Reason), cancellationToken);
        return NoContent();
    }
}
