using MediatR;
using Microsoft.Extensions.Logging;
using OrderPulse.Application.Common.Interfaces;
using OrderPulse.Domain.Events;

namespace OrderPulse.Application.Features.Orders.Events;

/// <summary>
/// Domain Event Handler responding to OrderCreatedDomainEvent.
/// In Laravel: Like an Event Listener in app/Listeners (e.g. SendOrderNotificationListener.php).
/// It handles side-effects: invalidating cached product catalog and queuing async email jobs.
/// </summary>
public sealed class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly ICacheService _cacheService;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<OrderCreatedDomainEventHandler> _logger;

    public OrderCreatedDomainEventHandler(
        ICacheService cacheService,
        IBackgroundTaskQueue taskQueue,
        ILogger<OrderCreatedDomainEventHandler> logger)
    {
        _cacheService = cacheService;
        _taskQueue = taskQueue;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Order {OrderId} created for customer {CustomerId}. Total: {TotalAmount} {Currency}",
            notification.OrderId, notification.CustomerId, notification.TotalAmount, notification.Currency);

        // 1. Invalidate Redis Product Cache (since stock was reserved)
        await _cacheService.RemoveAsync("products:active", cancellationToken);
        _logger.LogInformation("Invalidated Redis cache 'products:active' following order creation.");

        // 2. Queue asynchronous background job (Mirroring Laravel's dispatch(new SendOrderConfirmationEmailJob(...)))
        await _taskQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
        {
            var logger = (ILogger<OrderCreatedDomainEventHandler>)sp.GetService(typeof(ILogger<OrderCreatedDomainEventHandler>))!;
            logger.LogInformation("[Background Job] Dispatching order confirmation email for Order {OrderId}...", notification.OrderId);

            // Simulating email dispatch latency
            await Task.Delay(500, ct);

            logger.LogInformation("[Background Job] Order confirmation email successfully sent for Order {OrderId}!", notification.OrderId);
        });
    }
}
