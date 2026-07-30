namespace Sales.Domain.Events;

public record PaymentRejectedEvent
(
    Guid PaymentId,
    Guid OrderId,
    decimal Value,
    DateTime PaymentDate,
    string? TransactionCode
) 
: DomainEventBase;