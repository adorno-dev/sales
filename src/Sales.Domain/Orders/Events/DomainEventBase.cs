namespace Sales.Domain.Orders.Events;

public abstract record class DomainEventBase : IDomainEvent
{
    public DateTime OccurrenceDate { get; protected set; } = DateTime.UtcNow;
}