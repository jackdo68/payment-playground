using PaymentApp.Domain.Events;

namespace PaymentApp.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Domain events that occurred on this entity
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Creates an instance of any entity type.
    /// This works because T exists at runtime (reified generics).
    /// In TypeScript, this is impossible — T is erased.
    /// </summary>
    public static T Create<T>() where T : BaseEntity, new()
    {
        var entity = new T
        {
            CreatedAt = DateTime.UtcNow
        };

        Console.WriteLine($"Created entity of type: {typeof(T).Name}");
        return entity;
    }
}