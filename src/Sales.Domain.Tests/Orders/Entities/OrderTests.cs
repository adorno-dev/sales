using System.Reflection;
using FluentAssertions;
using Sales.Domain.Common.Enums;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Events;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Tests.Entities;

public class OrderTests
{
    private static ShippingAddress CreateValidShippingAddress()
        => ShippingAddress.Create
        (
            "12345-000",
            "Rua X",
            "Ap 1",
            "Centro",
            "SP",
            "Sao Paulo",
            "Brasil"
        );

    private static readonly Guid ValidCustomerId = Guid.NewGuid();
    private static readonly Guid ValidProductId = Guid.NewGuid();

    private static void SetOrderStatus(Order order, OrderStatus orderStatus)
    {
        typeof(Order).GetProperty(nameof(Order.OrderStatus), 
                        BindingFlags.Public | BindingFlags.Instance
                     )!.SetValue(order, orderStatus);
    }

    [Fact(DisplayName = "Should create a valid order with pending status")]
    public void Should_Create_Valid_Order()
    {
        // act
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        // assert
        order.Should().NotBeNull();
        order.CustomerId.Should().Be(ValidCustomerId);
        order.ShippingAddress.Should().NotBeNull();
        order.OrderStatus.Should().Be(OrderStatus.Pending);
        order.TotalValue.Should().Be(0);
        order.Items.Should().BeEmpty();
        order.Payments.Should().BeEmpty();
        order.Id.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Should not create order with invalid customer id")]
    public void ShouldNot_Create_Order_WithInvalidCustomerId()
    {
        // act
        Action act = () => Order.Create(Guid.Empty, CreateValidShippingAddress());
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Invalid customer id.");
    }

    [Fact(DisplayName = "Should not create order without a shipping address")]
    public void ShouldNot_Create_Order_WithoutShippingAddress()
    {
        // act
        Action act = () => Order.Create(ValidCustomerId, null!);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("The shipping address is required.");
    }

    [Fact(DisplayName = "Should add order item")]
    public void ShouldAddItemOrder()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        // act
        order.AddOrderItem(ValidProductId, "Mouse", 100m, 2);
        // assert
        order.Items.Should().HaveCount(1);
        order.TotalValue.Should().Be(200m);
        order.Items.First().TotalAmount.Should().Be(200m);
    }

    [Fact(DisplayName = "Should sum quantity of item on add the same product")]
    public void Should_Sum_Quantity_Items()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        var productId = ValidProductId;
        // act
        order.AddOrderItem(productId, "Keyboard", 200m, 1);
        order.AddOrderItem(productId, "Keyboard", 200m, 2);
        // assert
        order.Items.Should().HaveCount(1);
        var item = order.Items.First();
        item.Quantity.Should().Be(3);
        item.TotalAmount.Should().Be(600m);
        order.TotalValue.Should().Be(600m);
    }

    [Theory(DisplayName = "Should not allow add order items when the order status is not pending")]
    [InlineData(OrderStatus.PaymentConfirmed)]
    [InlineData(OrderStatus.Picking)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Canceled)]
    public void ShouldNot_AddOrderItem_WhenOrderStatusIsNotPending(OrderStatus orderStatus)
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        SetOrderStatus(order, orderStatus);
        // act
        Action act = () => order.AddOrderItem(Guid.NewGuid(), "Other", 100m, 1);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Items can be added only when the order status is pending.");
    }

    [Fact(DisplayName = "Should remove order item and recalculate total value")]
    public void Should_RemoveOrderItem_And_RecalculateTotalValue()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Mouse", 100m, 2);
        // act
        var act = () => order.RemoveOrderItem(order.Items.First().Id);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("The order should have at least one order item.");
    }

    [Fact(DisplayName = "Should remove order item and recalculate total value when it have more than one item")]
    public void Should_RemoveOrderItem_WhenOrderContainsMoreThanOneItem()
    {
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();

        order.AddOrderItem(product1, "Mouse", 100m, 1);
        order.AddOrderItem(product2, "Keyboard", 200m, 1);

        var itemId = order.Items.First(i => i.ProductId == product1).Id;
        order.RemoveOrderItem(itemId);

        order.Items.Should().HaveCount(1);
        order.TotalValue.Should().Be(200m);
    }

    [Fact(DisplayName = "Should ignore remove order item if it not exists")]
    public void ShouldIgnore_RemoveOrder_IfNotExists()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Mouse", 100m, 2);
        // act
        var act = () => order.RemoveOrderItem(Guid.NewGuid());
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Cannot find order item.");
    }

    [Theory(DisplayName = "Should not allow remove order item when it is not pending")]
    [InlineData(OrderStatus.PaymentConfirmed)]
    [InlineData(OrderStatus.Picking)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Canceled)]
    public void ShouldNot_RemoveOrderItem_WhenItIsNotPending(OrderStatus orderStatus)
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Product", 10m, 1);
        SetOrderStatus(order, orderStatus);
        // act
        var act = () => order.RemoveOrderItem(ValidProductId);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Items can be removed only when the order status is pending.");
    }

    // Shipping Address

    [Fact(DisplayName = "Should update shipping address when order status is pending")]
    public void Should_UpdateShippingAddress_WhenOrderStatusIsPending()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        var newShippingAddress = ShippingAddress.Create
        (
            "00000-000", 
            "New Street", 
            "Home",
            "New Neighborhood",
            "NS",
            "New Citty",
            "New Country"
        );
        // act
        order.UpdateShippingAddress(newShippingAddress);
        // assert
        order.ShippingAddress.Should().Be(newShippingAddress);
    }

    [Theory(DisplayName = "Should not update shipping addres when order status is not pending")]
    [InlineData(OrderStatus.PaymentConfirmed)]
    [InlineData(OrderStatus.Picking)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Canceled)]
    public void ShouldNot_UpdateShippingAddress_WhenOrderStatusIsNotPending(OrderStatus orderStatus)
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        ShippingAddress newShippingAddress = ShippingAddress.Create
        (
            "00000-000", 
            "New Street", 
            "Home",
            "New Neighborhood",
            "NS",
            "New Citty",
            "New Country"
        );
        SetOrderStatus(order, orderStatus);
        // act
        Action act = () => order.UpdateShippingAddress(newShippingAddress);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("The shipping address can only be changed when the order is pending");
    }

    // Payments

    [Fact(DisplayName = "Should start payment and keeping order status on picking")]
    public void Should_Start_Payment()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Product", 100m, 2);
        // act
        var payment = order.StartPayment(PaymentMethod.CreditCard);
        // assert
        payment.Should().NotBeNull();
        payment.Value.Should().Be(200m);
        order.Payments.Should().Contain(payment);
        order.OrderStatus.Should().Be(OrderStatus.Pending);
    }

    [Fact(DisplayName = "Should not start payment when the order has no items")]
    public void ShouldNot_StartPayment_WithoutOrderItems()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        // act
        var act = () => order.StartPayment(PaymentMethod.Pix);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("An order must contain at least one item before payment can be started.");
    }

    [Fact(DisplayName = "Should not start payment if already exists another pending payment")]
    public void ShouldNot_StartPayment_IfAlreadyExistsAnotherPendingPayment()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Product", 100m, 1);
        // simulates a creation of pending payment
        order.StartPayment(PaymentMethod.Pix);
        // act
        var act = () => order.StartPayment(PaymentMethod.CreditCard);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("A pending payment already exists for this order.");
    }

    [Fact(DisplayName = "Should set order status on handle approved payment")]
    public void Should_SetOrderStatus_OnHandlePaymentApproved()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Product", 100m, 1);
        var payment = order.StartPayment(PaymentMethod.Pix);
        // act
        order.HandlePaymentApproved(payment.Id);
        // assert
        order.OrderStatus.Should().Be(OrderStatus.PaymentConfirmed);
    }

    [Fact(DisplayName = "Should keep order status to pending on handle rejected payment")]
    public void Should_KeepOrderStatusPending_OnHandlePaymentRejected()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Product", 100m, 1);
        var payment = order.StartPayment(PaymentMethod.Pix);
        // act
        order.HandlePaymentRejected(payment.Id);
        // assert
        order.OrderStatus.Should().Be(OrderStatus.Canceled);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelledEvent);
    }

    [Fact(DisplayName = "Should not handle approved payment when order status is not pending")]
    public void ShouldNot_HandlePaymentApproved_WhenOrderStatusIsNotPending()
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Product", 100m, 1);
        Payment payment = order.StartPayment(PaymentMethod.Pix);
        SetOrderStatus(order, OrderStatus.Picking);
        // act
        Action act = () => order.HandlePaymentApproved(payment.Id);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Payment confirmation is only allowed for pending orders.");
    }

    // Order status transitions

    [Fact(DisplayName = "Should allow order status marked as picking to payment confirmation")]
    public void Should_MarkAsPicking()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Product", 100m, 1);
        var payment = order.StartPayment(PaymentMethod.CreditCard);
        order.HandlePaymentApproved(payment.Id);
        // act
        order.MarkAsPicking();
        // assert
        order.OrderStatus.Should().Be(OrderStatus.Picking);
    }

    [Fact(DisplayName = "Should not mark as picking if payment is not confirmed")]
    public void ShouldNot_MarkOrderStatusPicking_IfPaymentIsNotConfirmed()
    {
        // arrange
        var order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        // act
        var act = () => order.MarkAsPicking();
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Order can only enter picking after payment confirmation.");
    }

    [Fact(DisplayName = "Should mark as shipped")]
    public void Should_MarkAsShipped()
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        SetOrderStatus(order, OrderStatus.Picking);
        // act
        order.MarkAsShipped();
        // assert
        order.OrderStatus.Should().Be(OrderStatus.Shipped);
    }

    [Fact(DisplayName = "Should not mark as shipped if order status is not in picking")]
    public void ShouldNot_MarkAsShipped_IfOrderStatusItNotInPicking()
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        SetOrderStatus(order, OrderStatus.PaymentConfirmed);
        // act
        Action act = () => order.MarkAsShipped();
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Order can only be shipped after it has been picked.");
    }

    [Fact(DisplayName = "Should mark as delivered")]
    public void Should_MarkAsDelivered()
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        SetOrderStatus(order, OrderStatus.Shipped);
        // act
        order.MarkAsDelivered();
        // assert
        order.OrderStatus.Should().Be(OrderStatus.Delivered);
    }

    [Fact(DisplayName = "Should not mark as delivered if order status is not shipped")]
    public void ShouldNot_MarkAsDelivered_IfOrderStatusIsNotShipped()
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        SetOrderStatus(order, OrderStatus.Picking);
        // act
        Action act = () => order.MarkAsDelivered();
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Order can only be delivered after it has been shipped.");
    }

    // Cancellation order

    [Fact(DisplayName = "Should cancel order when order status is pending")]
    public void Should_Cancel_Order_WhenOrderStatusIsPending()
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Product", 50m, 1);
        // act
        order.CancelOrder();
        // assert
        order.OrderStatus.Should().Be(OrderStatus.Canceled);
    }

    [Fact(DisplayName = "Should cancel order when order status is payment confirmed")]
    public void Should_CancelOrder_WhenOrderStatusIsPaymentConfirmed()
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        order.AddOrderItem(ValidProductId, "Product", 50m, 1);
        Payment payment = order.StartPayment(PaymentMethod.Pix);
        order.HandlePaymentApproved(payment.Id);
        // act
        order.CancelOrder();
        // assert
        order.OrderStatus.Should().Be(OrderStatus.Canceled);
    }

    [Theory(DisplayName = "Should not cancel order after order status is picking")]
    [InlineData(OrderStatus.Picking)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    public void ShouldNot_CancelOrder_AfterOrderStatusIsPicking(OrderStatus orderStatus)
    {
        // arrange
        Order order = Order.Create(ValidCustomerId, CreateValidShippingAddress());
        SetOrderStatus(order, orderStatus);
        // act
        Action act = () => order.CancelOrder();
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Order cannot be cancelled once it has been picking or any later stage.");
    }
}