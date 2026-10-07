namespace OrderPulse.Domain.Repositories;

/// <summary>
/// Unit of Work pattern coordinates writes and transaction commits.
/// In Laravel: DB::transaction(function() { ... })
/// In .NET: The DbContext implements IUnitOfWork and commits all pending changes atomically.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
