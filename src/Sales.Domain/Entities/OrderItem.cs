using Sales.Domain.Common.Base;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.Common.Validations;

namespace Sales.Domain.Entities;

public sealed class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal AppliedDiscount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // private OrderItem() {}

    internal OrderItem
    (
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity
    )
    {
        Guard.AgainstEmptyGuid(productId, nameof(productId), "Invalid product id.");
        Guard.AgainstNullOrWhiteSpace(productName, nameof(productName), "The product name is required.");
        Guard.Against<DomainException>(unitPrice <= 0, "The unit price should be greater than zero.");
        Guard.Against<DomainException>(quantity <= 0, "The quantity should be greater than zero.");

        (ProductId, ProductName, UnitPrice, Quantity, AppliedDiscount) =
        (productId, productName, unitPrice, quantity, 0);

        CalculateTotal();
    }

    public void ApplyDiscount(decimal discount)
    {
        Guard.Against<DomainException>(discount < 0, "Discount cannot be negative.");
        Guard.Against<DomainException>(discount > UnitPrice * Quantity, "Discount cannot exceed the total.");

        AppliedDiscount = discount;
        SetUpdatedAt();
        CalculateTotal();
    }

    public void AddUnits(int units)
    {
        Guard.Against<DomainException>(units <= 0, "Units should be added at least one or more.");
        Quantity += units;
        SetUpdatedAt();
        CalculateTotal();
    }

    public void RemoveUnits(int units)
    {
        Guard.Against<DomainException>(units <= 0, "Should remove at least one unit.");
        Guard.Against<DomainException>
        (
            condition: units > Quantity, 
            message: "Cannot remove more units than are available in the item."
        );
        Quantity -= units;
        Guard.Against<DomainException>
        (
            condition: Quantity == 0, 
            message: "An order item cannot have zero quantity. (Use the method from Order class to remove it)"
        );
        SetUpdatedAt();
        CalculateTotal();
    }

    public void UpdateUnitPrice(decimal newPrice)
    {
        Guard.Against<DomainException>(newPrice <= 0, "The unit price should be greater then zero.");
        UnitPrice = newPrice;
        SetUpdatedAt();
        CalculateTotal();
    }

    private void CalculateTotal()
        => TotalAmount = (UnitPrice * Quantity) - AppliedDiscount;
}