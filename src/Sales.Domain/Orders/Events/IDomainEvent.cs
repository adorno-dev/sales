namespace Sales.Domain.Orders.Events;

public interface IDomainEvent
{
    DateTime OccurrenceDate { get; }
}