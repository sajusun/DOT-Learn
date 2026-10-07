namespace OrderPulse.Application.Common.Interfaces;

/// <summary>
/// Asynchronous background task queue interface.
/// In Laravel: Like dispatch(new SendEmailJob($orderId)) pushing to Redis queue.
/// </summary>
public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<IServiceProvider, CancellationToken, ValueTask> workItem);
    ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}
