using MediatR;

namespace OrderPulse.Domain.Common;

/// <summary>
/// Marker interface for Domain Events.
/// Inherits INotification so MediatR can dispatch it to event handlers (subscribers).
/// In Laravel terms: Equivalent to an event class in app/Events (e.g. OrderCreated.php)
/// listened to by listeners in app/Listeners.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOnUtc { get; }
}
