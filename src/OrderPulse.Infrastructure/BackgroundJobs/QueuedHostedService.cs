using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderPulse.Application.Common.Interfaces;

namespace OrderPulse.Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker processing queued tasks.
/// In Laravel: Corresponds directly to php artisan queue:work.
/// Runs in the background of the web application or worker process,
/// creating an isolated IServiceScope for each job.
/// </summary>
public sealed class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Queue Worker has started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                // Create a scoped service container for the job (so scoped services like DbContext are cleanly disposed)
                using var scope = _serviceProvider.CreateScope();

                await workItem(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful host shutdown requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing queued background work item.");
            }
        }

        _logger.LogInformation("Background Queue Worker is shutting down.");
    }
}
