namespace OrderPulse.Domain.Exceptions;

/// <summary>
/// Base exception for business logic violations in the domain layer.
/// In Laravel: Like throwing a custom domain exception caught in bootstrap/app.php.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }

    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class InsufficientStockException : DomainException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Insufficient stock for product '{productId}'. Requested: {requested}, Available: {available}.")
    {
        ProductId = productId;
        RequestedQuantity = requested;
        AvailableQuantity = available;
    }
}

public sealed class InvalidOrderStateTransitionException : DomainException
{
    public InvalidOrderStateTransitionException(string currentStatus, string targetStatus)
        : base($"Cannot transition order from status '{currentStatus}' to '{targetStatus}'.")
    {
    }
}

public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with identifier '{key}' was not found.")
    {
    }
}
