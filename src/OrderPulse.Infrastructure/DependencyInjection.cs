using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderPulse.Application.Common.Interfaces;
using OrderPulse.Domain.Repositories;
using OrderPulse.Infrastructure.Caching;
using OrderPulse.Infrastructure.Persistence;
using OrderPulse.Infrastructure.Persistence.Repositories;

namespace OrderPulse.Infrastructure;

/// <summary>
/// Infrastructure Service Provider registration.
/// In Laravel: Like App\Providers\InfrastructureServiceProvider.php registering bindings in register().
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Connection (EF Core + PostgreSQL)
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=orderpulse_db;Username=postgres;Password=postgrespassword";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Redis Distributed Cache
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "OrderPulse:";
        });

        services.AddSingleton<ICacheService, DistributedCacheService>();

        // JWT Token Generation
        services.Configure<Identity.JwtSettings>(configuration.GetSection(Identity.JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenGenerator, Identity.JwtTokenGenerator>();

        // Background Task Queue & Worker (Laravel Queue & Worker equivalent)
        services.AddSingleton<IBackgroundTaskQueue, BackgroundJobs.BackgroundTaskQueue>();
        services.AddHostedService<BackgroundJobs.QueuedHostedService>();

        return services;
    }
}
