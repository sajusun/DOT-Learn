using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OrderPulse.Application.Common.Exceptions;
using OrderPulse.Domain.Exceptions;

namespace OrderPulse.Api.Middleware;

/// <summary>
/// Global Exception Handling Middleware.
/// In Laravel: Corresponds to bootstrap/app.php -> withExceptions() / App\Exceptions\Handler.php.
/// Formats errors compliant with RFC 7807 (application/problem+json).
/// </summary>
public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/problem+json";

        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ValidationProblemDetails(validationEx.Errors)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Instance = context.Request.Path
                }
            ),

            EntityNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Resource Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path
                }
            ),

            DomainException domainEx => (
                StatusCodes.Status422UnprocessableEntity,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                    Title = "Domain Rule Violation",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = domainEx.Message,
                    Instance = context.Request.Path
                }
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred. Please contact system support.",
                    Instance = context.Request.Path
                }
            )
        };

        response.StatusCode = statusCode;
        await response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
