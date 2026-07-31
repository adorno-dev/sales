using Sales.Domain.Common.Base;
using Sales.Domain.Common.Exceptions;

namespace Sales.Domain.Orders.ValueObjects;

public sealed class OrderCancellationReason : ValueObject
{
    public string Code { get; private set; }
    public string Description { get; private set; }

    // Conjunto de motivos padronizados no dominio
    private static readonly Dictionary<string, string> _defaultReasons = new()
    {
        { "CustomerCancelled", "Customer cancelled the purchase" },
        { "PaymentError", "Payment processing error" },
        { "OutOfStock", "Item is out of stock" },
        { "InvalidDeliveryAddress", "Invalid delivery address" },
        { "Other", "Other unspecified reason" }
    };

    // Construtor
    public OrderCancellationReason(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("The cancellation reason code is required.");
        
        if (!_defaultReasons.ContainsKey(code))
            throw new DomainException($"The cancellation reason code '{code}' is invalid.");
        
        Code = code;
        Description = _defaultReasons[code];
    }

    // Metodo de fabrica para cada motivo comum
    public static OrderCancellationReason CustomerCancelled => new ("CustomerCancelled");
    public static OrderCancellationReason PaymentError => new ("PaymentError");
    public static OrderCancellationReason OutOfStock => new ("OutOfStock");
    public static OrderCancellationReason InvalidDeliveryAddress => new ("InvalidDeliveryAddress");
    public static OrderCancellationReason Other => new ("Other");

    // Igualdade estrutural
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
        yield return Description;
    }

    public override string ToString() => $"{Description}";
}