using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderPulse.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior to measure execution time and provide structured logs.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Processing {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            _logger.LogWarning("Long Running Request: {RequestName} took {ElapsedMilliseconds}ms",
                requestName, elapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName, elapsedMilliseconds);
        }

        return response;
    }
}
