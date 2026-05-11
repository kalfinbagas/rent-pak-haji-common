namespace RentPakHaji.Common.Domain.Events;

/// <summary>
/// Base record for domain events. Extend this in each service.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
