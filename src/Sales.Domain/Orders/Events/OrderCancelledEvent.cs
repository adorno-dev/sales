using Sales.Domain.Common.Enums;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders.Events;

public sealed record OrderCancelledEvent
(
    Guid OrderId,
    Guid CustomerId,
    OrderStatus PreviousStatus,
    OrderCancellationReason Reason,
    Guid? PaymentId
)
: DomainEventBase;