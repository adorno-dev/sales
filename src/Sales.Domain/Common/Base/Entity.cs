using Sales.Domain.Orders.Events;

namespace Sales.Domain.Common.Base;

public abstract class Entity(Guid? id = null)
{
    // A propria classe ou classes derivadas podem alterar esses valores.
    public Guid Id { get; protected set; } = id ?? Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    // protected Entity(Guid id) => Id = id;

    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        return GetType() == other.GetType() &&
               Id == other.Id;
    }

    public override int GetHashCode() =>
        HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right) =>
        !(left == right);

    // 

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    protected void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    protected void ClearDomainEvents() => _domainEvents.Clear();
}