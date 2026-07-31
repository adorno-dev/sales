using System.Data.Common;
using Sales.Domain.Common.Base;
using Sales.Domain.Common.Enums;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.Common.Validations;
using Sales.Domain.Orders.Events;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders.Entities;

public sealed class Order : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public ShippingAddress ShippingAddress { get; private set; }
    public decimal TotalValue { get; private set; }
    public OrderStatus OrderStatus { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private Order(Guid customerId, ShippingAddress shippingAddress)
    {
        Guard.AgainstEmptyGuid
        (
            customerId, 
            nameof(customerId), 
            "Invalid customer id."
        );
        Guard.AgainstNull
        (
            shippingAddress,
            nameof(shippingAddress),
            "The shipping address is required."
        );

        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        OrderStatus = OrderStatus.Pending;
        TotalValue = 0m;

        GenerateOrderNumber();
    }

    private void GenerateOrderNumber()
        => OrderNumber = $"ORDER-{Id.ToString()[..8].ToUpper()}";
    
    public static Order Create
    (
        Guid customerId,
        ShippingAddress shippingAddress
    )
    => new (customerId, shippingAddress);

    public void AddOrderItem
    (
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity
    )
    {
        Guard.Against<DomainException>
        (
            OrderStatus != OrderStatus.Pending,
            "Items can be added only when the order status is pending."
        );

        var exists = _items.FirstOrDefault(item => item.ProductId == productId);
        if (exists is not null)
            exists.AddUnits(quantity);
        else _items.Add(new(productId, productName, unitPrice, quantity));

        RecalculateTotalValue();
        SetUpdatedAt();
    }

    public void RemoveOrderItem(Guid itemId)
    {
        Guard.AgainstEmptyGuid(itemId, nameof(itemId), "Invalid order item id");
        Guard.Against<DomainException>
        (
            OrderStatus != OrderStatus.Pending,
            "Items can be removed only when the order status is pending."
        );

        OrderItem? orderItem = _items.FirstOrDefault(item => item.Id == itemId);
        Guard.AgainstNull(orderItem, nameof(orderItem), "Cannot find order item.");

        _items.Remove(orderItem!);

        Guard.Against<DomainException>
        (
            _items.Count == 0,
            "The order should have at least one order item."
        );

        RecalculateTotalValue();
        SetUpdatedAt();
    }

    private void RecalculateTotalValue()
        => TotalValue = _items.Sum(item => item.TotalAmount);

    public void UpdateShippingAddress(ShippingAddress newShippingAddress)
    {
        Guard.AgainstNull(newShippingAddress, nameof(newShippingAddress));
        Guard.Against<DomainException>
        (
            OrderStatus != OrderStatus.Pending,
            "The shipping address can only be changed when the order is pending"
        );

        ShippingAddress = newShippingAddress;
        SetUpdatedAt();
    }

    public Payment StartPayment(PaymentMethod paymentMethod)
    {
        Guard.Against<DomainException>
        (
            !_items.Any(),
            "An order must contain at least one item before payment can be started."
        );

        Guard.Against<DomainException>
        (
            OrderStatus != OrderStatus.Pending,
            "Payment can only be started when the order status is pending."
        );

        if (_payments.Any(payment => payment.PaymentStatus == PaymentStatus.Pending))
            throw new DomainException("A pending payment already exists for this order.");

        var newPayment = new Payment(Id, paymentMethod, TotalValue);

        _payments.Add(newPayment);

        SetUpdatedAt();

        return newPayment;
    }

    public void HandlePaymentApproved(Guid paymentId)
    {
        Payment? payment = _payments.FirstOrDefault(p => p.Id == paymentId);

        if (payment is null) return;

        Guard.Against<DomainException>
        (
            OrderStatus != OrderStatus.Pending,
            "Payment confirmation is only allowed for pending orders."
        );

        OrderStatus = OrderStatus.PaymentConfirmed;
        SetUpdatedAt();
    }

    public void HandlePaymentRejected(Guid paymentId)
    {
        Payment? payment = _payments.FirstOrDefault(p => p.Id == paymentId);

        if (payment is null) return;

        Guard.Against<DomainException>
        (
            OrderStatus != OrderStatus.Pending,
            "Payment rejection can only be processed for pending orders."
        );

        OrderStatus = OrderStatus.Canceled;
        SetUpdatedAt();

        AddDomainEvent(new OrderCancelledEvent(
            Id,
            CustomerId,
            OrderStatus,
            OrderCancellationReason.PaymentError,
            payment.Id
        ));
    }

    public void MarkAsPicking()
    {
        Guard.Against<DomainException>
        (
            OrderStatus != OrderStatus.PaymentConfirmed,
            "Order can only enter picking after payment confirmation."
        );

        OrderStatus = OrderStatus.Picking;
        SetUpdatedAt();
    }

    public void MarkAsShipped()
    {
        Guard.Against<DomainException>
        (
            OrderStatus != OrderStatus.Picking,
            "Order can only be shipped after it has been picked."
        );

        OrderStatus = OrderStatus.Shipped;
        SetUpdatedAt();

        AddDomainEvent(new OrderShippedEvent(Id, CustomerId, ShippingAddress));
    }

    public void MarkAsDelivered()
    {
        Guard.Against<DomainException>
        (
            OrderStatus != OrderStatus.Shipped,
            "Order can only be delivered after it has been shipped."
        );

        OrderStatus = OrderStatus.Delivered;
        SetUpdatedAt();

        AddDomainEvent(new OrderDeliveredEvent(Id, CustomerId));
    }

    public void CancelOrder(OrderCancellationReason? reason = null)
    {
        Guard.Against<DomainException>
        (
            OrderStatus >= OrderStatus.Picking,
            "Order cannot be cancelled once it has been picking or any later stage."
        );

        OrderStatus = OrderStatus.Canceled;
        SetUpdatedAt();

        AddDomainEvent(new OrderCancelledEvent
        (
            Id,
            CustomerId,
            OrderStatus,
            reason ?? OrderCancellationReason.Other,
            _payments.LastOrDefault()?.Id
        ));
    }
}