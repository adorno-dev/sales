using Sales.Domain.ValueObjects;

namespace Sales.Domain.Events;

public sealed record OrderShippedEvent
(
    Guid OrderId,
    Guid CustomerId,
    ShippingAddress ShippingAddress
)
: DomainEventBase;
