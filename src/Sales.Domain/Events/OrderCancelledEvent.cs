using Sales.Domain.Common.Enums;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Events;

public sealed record OrderCancelledEvent
(
    Guid OrderId,
    Guid CustomerId,
    OrderStatus PreviousStatus,
    OrderCancellationReason Reason,
    Guid? PaymentId
)
: DomainEventBase;