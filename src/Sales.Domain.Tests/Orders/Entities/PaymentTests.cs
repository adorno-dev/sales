using FluentAssertions;
using Sales.Domain.Common.Enums;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Events;

namespace Sales.Domain.Tests.Entities;

public class PaymentTests
{
    [Fact(DisplayName = "Should create a valid payment with pending status")]
    public void ShouldCreate_ValidPayment_WithPendingStatus()
    {
        var orderId = Guid.NewGuid();
        var method = PaymentMethod.CreditCard;
        var value = 100m;

        var payment = new Payment(orderId, method, value);

        payment.OrderId.Should().Be(orderId);
        payment.PaymentMethod.Should().Be(method);
        payment.Value.Should().Be(value);
        payment.PaymentStatus.Should().Be(PaymentStatus.Pending);
        payment.PaymentDate.Should().BeNull();
        payment.TransactionCode.Should().BeNull();
    }

    [Fact(DisplayName = "Should not create payment with values less than zero.")]
    public void ShouldNotCreate_Payment_WithInvalidValue()
    {
        Guid orderId = Guid.NewGuid();
        Action act = () => new Payment(orderId, PaymentMethod.Pix, 0);
        act.Should()
           .Throw<DomainException>()
           .WithMessage("The payment value should be greater than zero.");
    }

    [Fact(DisplayName = "Should not define transaction code with null or empty")]
    public void ShouldNotDefine_TransactionCode_WithNullOrEmpty()
    {
        Payment payment = new (Guid.NewGuid(), PaymentMethod.Pix, 100m);
        Action act = () => payment.DefineTransactionCode("");
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Invalid transaction code.");
    }

    [Fact(DisplayName = "Should define valid transaction code and update UpdatedAt")]
    public void ShouldDefine_ValidTransactionCode()
    {
        var payment = new Payment(Guid.NewGuid(), PaymentMethod.CreditCard, 100m);
        var transactionCode = "TXN-12345";

        payment.DefineTransactionCode(transactionCode);
        payment.TransactionCode.Should().Be(transactionCode);
        payment.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Should not redefine transaction code already defined")]
    public void ShouldNotRedefine_TransactionCode()
    {
        Payment payment = new (Guid.NewGuid(), PaymentMethod.CreditCard, 100m);
        payment.DefineTransactionCode("TXN-001");
        Action act = () => payment.DefineTransactionCode("TXN-002");
        act.Should()
           .Throw<DomainException>()
           .WithMessage("The transaction code already defined.");
    }

    [Fact(DisplayName = "Should generate local transaction code automatically")]
    public void ShouldGenerate_LocalTransactionCode()
    {
        var payment = new Payment(Guid.NewGuid(), PaymentMethod.Pix, 200m);
        payment.GenerateLocalTransactionCode();
        payment.TransactionCode.Should().StartWith("LOCAL-");
        payment.TransactionCode.Should().HaveLength(14);    // LOCAL- + 8 chars
        payment.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Should confirm pending payment with a valid transaction code and generate complete event")]
    public void ShoudConfirmPendingPayment_WithValidTransactionCodeAndGenerateCompleteEvent()
    {
        var payment = new Payment(Guid.NewGuid(), PaymentMethod.CreditCard, 300m);
        payment.GenerateLocalTransactionCode();     // Simulate gateway
        payment.ConfirmPayment();
        payment.PaymentStatus.Should().Be(PaymentStatus.Approved);
        payment.PaymentDate.Should().NotBeNull();
        payment.UpdatedAt.Should().NotBeNull();

        var approvedEvent = payment.DomainEvents.OfType<PaymentApprovedEvent>().FirstOrDefault();
        approvedEvent.Should().NotBeNull();
        approvedEvent!.PaymentId.Should().Be(payment.Id);
        approvedEvent.OrderId.Should().Be(payment.OrderId);
        approvedEvent.Value.Should().Be(payment.Value);
        approvedEvent.TransactionCode.Should().Be(payment.TransactionCode);
        approvedEvent.PaymentDate.Should().Be(payment.PaymentDate);
    }

    [Fact(DisplayName = "Should not confirm payment without transaction code")]
    public void ShouldNotConfirm_WithoutTransactionCode()
    {
        Payment payment = new (Guid.NewGuid(), PaymentMethod.Pix, 100m);
        Action act = () => payment.ConfirmPayment();
        act.Should()
           .Throw<DomainException>()
           .WithMessage("The payment should not be confirmed without a transaction code.");
    }

    [Fact(DisplayName = "Should not confirm not pending payment status")]
    public void ShouldNotConfirm_NotPendingPaymentStatus()
    {
        Payment payment = new (Guid.NewGuid(), PaymentMethod.Pix, 100m);
        payment.GenerateLocalTransactionCode();
        payment.ConfirmPayment();

        Action act = () => payment.ConfirmPayment();

        act.Should()
           .Throw<DomainException>()
           .WithMessage("Only pending payments can be confirmed.");
    }

    [Fact(DisplayName = "Should decline pending payment and generate event with data")]
    public void ShouldDecline_PendingPayment_And_GenerateEventWithData()
    {
        Payment payment = new (Guid.NewGuid(), PaymentMethod.Pix, 120m);
        payment.DeclinePayment();
        payment.PaymentStatus.Should().Be(PaymentStatus.Declined);
        payment.PaymentDate.Should().NotBeNull();
        payment.UpdatedAt.Should().NotBeNull();

        PaymentRejectedEvent? paymentEvent = payment.DomainEvents.OfType<PaymentRejectedEvent>().FirstOrDefault();
        paymentEvent.Should().NotBeNull();
        paymentEvent!.PaymentId.Should().Be(payment.Id);
        paymentEvent.OrderId.Should().Be(payment.OrderId);
        paymentEvent.Value.Should().Be(payment.Value);
        paymentEvent.TransactionCode.Should().Be(paymentEvent.TransactionCode);
        paymentEvent.PaymentDate.Should().Be(payment.PaymentDate);
    }

    [Fact(DisplayName = "Should not decline payment when status is not pending")]
    public void ShouldNotDeclinePayment_WhenStatusIsNotPending()
    {
        Payment payment = new (Guid.NewGuid(), PaymentMethod.Pix, 120m);
        payment.GenerateLocalTransactionCode();
        payment.ConfirmPayment();

        Action act = () => payment.DeclinePayment();

        act.Should()
           .Throw<DomainException>()
           .WithMessage("Only pending payments can be rejected.");
    }
}