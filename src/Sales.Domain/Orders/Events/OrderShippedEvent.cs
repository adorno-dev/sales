using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders.Events;

public sealed record OrderShippedEvent
(
    Guid OrderId,
    Guid CustomerId,
    ShippingAddress ShippingAddress
)
: DomainEventBase;
