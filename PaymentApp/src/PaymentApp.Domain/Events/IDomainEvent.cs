namespace PaymentApp.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// Events describe something that happened in the domain.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

/// <summary>
/// Base record for domain events with automatic timestamp.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}