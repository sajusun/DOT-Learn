using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderPulse.Domain.Common;
using OrderPulse.Domain.Entities;
using OrderPulse.Domain.Repositories;

namespace OrderPulse.Infrastructure.Persistence;

/// <summary>
/// The EF Core DbContext implements the Unit of Work pattern.
/// In Laravel: Like DB::connection() + Eloquent repository combined,
/// tracking all entity states (Added, Modified, Unchanged, Deleted) in memory
/// and generating a single atomic SQL transaction upon SaveChangesAsync().
/// </summary>
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher? _publisher;

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher? publisher = null)
        : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically scan and apply all IEntityTypeConfiguration classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Automatic Timestamping (Mirroring Laravel's Eloquent $timestamps = true)
        UpdateAuditTimestamps();

        // 2. Collect Domain Events before persisting
        var domainEvents = ExtractDomainEvents();

        // 3. Persist state changes in an atomic transaction
        var result = await base.SaveChangesAsync(cancellationToken);

        // 4. Publish Domain Events via MediatR (Mirroring Laravel's event(new ...))
        if (_publisher != null && domainEvents.Count > 0)
        {
            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }

        return result;
    }

    private void UpdateAuditTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity.GetType().BaseType is not null &&
                        e.State is EntityState.Modified);

        foreach (var entry in entries)
        {
            var updatedAtProp = entry.Property("UpdatedAtUtc");
            if (updatedAtProp != null)
            {
                updatedAtProp.CurrentValue = DateTime.UtcNow;
            }
        }
    }

    private List<IDomainEvent> ExtractDomainEvents()
    {
        var aggregateRoots = ChangeTracker.Entries()
            .Where(e => e.Entity.GetType().BaseType is not null)
            .Select(e => e.Entity)
            .OfType<AggregateRoot<Guid>>()
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregateRoots)
        {
            aggregate.ClearDomainEvents();
        }

        return domainEvents;
    }
}
