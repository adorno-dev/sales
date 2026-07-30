using Sales.Domain.Common.Base;
using Sales.Domain.Common.Enums;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.Common.Validations;
using Sales.Domain.Events;

namespace Sales.Domain.Entities;

public sealed class Payment : Entity
{
    public Guid OrderId { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public decimal Value { get; private set; }  // Value Object MonetaryValue
    public DateTime? PaymentDate { get; private set; }
    public string? TransactionCode { get; private set; }

    public Payment
    (
        Guid orderId,
        PaymentMethod paymentMethod,
        decimal value
    )
    {
        Guard.AgainstEmptyGuid(orderId, nameof(orderId), "Invalid order.");
        Guard.Against<DomainException>(value <= 0, "The payment value should be greater than zero.");
        Guard.Against<DomainException>
        (
            !Enum.IsDefined(typeof(PaymentMethod), paymentMethod),
            "Payment method is invalid."
        );

        (OrderId, PaymentMethod, Value) =
        (orderId, paymentMethod, value);

        // Initial payment status
        PaymentStatus = PaymentStatus.Pending;
        PaymentDate = null;
        TransactionCode = null;
    }

    public void GenerateLocalTransactionCode()
    {
        if (TransactionCode is not null) return;

        String localTransactionCode = $"LOCAL-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        DefineTransactionCode(localTransactionCode);
    }

    public void DefineTransactionCode(string transactionCode)
    {
        Guard.AgainstNullOrWhiteSpace(transactionCode, nameof(transactionCode), "Invalid transaction code.");
        Guard.Against<DomainException>(TransactionCode is not null, "The transaction code already defined.");
        Guard.Against<DomainException>
        (
            PaymentStatus != PaymentStatus.Pending,
            "Cannot register transaction code after confirmed or declined payment."
        );

        // Transaction code were generated only single time, when the payment is approved.
        TransactionCode = transactionCode;
        SetUpdatedAt();
    }

    public void ConfirmPayment()
    {
        Guard.Against<DomainException>
        (
            PaymentStatus != PaymentStatus.Pending, 
            "Only pending payments can be confirmed."
        );
        Guard.AgainstNullOrWhiteSpace
        (
            TransactionCode ?? string.Empty,
            nameof(TransactionCode),
            "The payment should not be confirmed without a transaction code."
        );

        PaymentStatus = PaymentStatus.Approved;
        PaymentDate = DateTime.UtcNow;
        SetUpdatedAt();
        AddDomainEvent(new PaymentApprovedEvent(Id, OrderId, Value, PaymentDate.Value, TransactionCode));
    }

    public void DeclinePayment()
    {
        Guard.Against<DomainException>
        (
            PaymentStatus != PaymentStatus.Pending,
            "Only pending payments can be rejected."
        );

        PaymentStatus = PaymentStatus.Declined;
        PaymentDate= DateTime.UtcNow;
        SetUpdatedAt();
        AddDomainEvent(new PaymentRejectedEvent(Id, OrderId, Value, PaymentDate.Value, TransactionCode));
    }
}