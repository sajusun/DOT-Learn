using OrderPulse.Api.Extensions;
using OrderPulse.Api.Middleware;
using OrderPulse.Application;
using OrderPulse.Infrastructure;
using OrderPulse.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog for structured logging (Replaces Laravel Monolog)
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console();
});

// 2. Register Application & Infrastructure Layers (Laravel Service Providers)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 3. Register Security & Documentation
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwtAuth();

// 4. Register Exception Handling Middleware
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// 5. Register Controllers
builder.Services.AddControllers();

var app = builder.Build();

// 6. Global Exception Middleware (Catches all exceptions, converts to RFC 7807 ProblemDetails)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 7. Serilog HTTP Request Logging (Emits structured log for every HTTP call with status code & duration)
app.UseSerilogRequestLogging();

// 8. Swagger UI Documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderPulse API v1");
        c.RoutePrefix = string.Empty; // Swagger UI served at root URL
    });
}

// 9. Authentication & Authorization Pipelines
app.UseAuthentication();
app.UseAuthorization();

// 10. Map Controller Endpoints
app.MapControllers();

// 11. Apply Migrations and Seed Demo Data on Startup (Laravel php artisan migrate --seed)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.SeedAsync(dbContext);
        logger.LogInformation("Database migration and seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database not available for immediate auto-migration at startup. Start Docker or verify connection.");
    }
}

app.Run();
