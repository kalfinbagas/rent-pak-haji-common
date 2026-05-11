using MediatR;

namespace RentPakHaji.Common.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// Implements INotification so MediatR can dispatch them.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
