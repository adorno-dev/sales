namespace Sales.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurrenceDate { get; }
}