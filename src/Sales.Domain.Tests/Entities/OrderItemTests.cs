using FluentAssertions;
using Sales.Domain.Common.Base;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.Entities;

namespace Sales.Domain.Tests.Entities;

public class OrderItemTests
{
    private static OrderItem CreateValidItem(decimal price = 100m, int quantity = 2)
        => new (Guid.NewGuid(), "Test Product", price, quantity);
    
    // Creation tests
    [Fact(DisplayName = "Should create OrderItem successfully when data are valid.")]
    public void Create_ShouldReturnOrderItem_WhenDataAreValid()
    {
        var productId = Guid.NewGuid();
        var productName = "Mechanic Keyboard";
        var unitPrice = 250m;
        var quantity = 2;

        var orderItem = new OrderItem
        (
            productId,
            productName,
            unitPrice,
            quantity
        );

        orderItem.ProductId.Should().Be(productId);
        orderItem.ProductName.Should().Be(productName);
        orderItem.UnitPrice.Should().Be(unitPrice);
        orderItem.Quantity.Should().Be(quantity);
        orderItem.AppliedDiscount.Should().Be(0);
        orderItem.TotalAmount.Should().Be(500m);
    }

    [Theory(DisplayName = "Should throw DomainException when parameters are invalid.")]
    [InlineData("", "Product A", 10, 1, "Invalid product id.")]
    [InlineData("guid", "", 10, 1, "The product name is required.")]
    [InlineData("guid", "Product B", 0, 1, "The unit price should be greater than zero.")]
    [InlineData("guid", "Product C", 10, 0, "The quantity should be greater than zero.")]
    public void Create_ShouldThrowDomainException_WhenParametersAreInvalid
    (
        string type,
        string productName,
        decimal price,
        int quantity,
        string message
    )
    {
        // arrange
        Guid productId = type == "guid" ? Guid.NewGuid() : Guid.Empty;
        // act
        Action act = () => new OrderItem(productId, productName, price, quantity);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage(message);
    }

    // Discount Tests
    [Fact(DisplayName = "Should apply discount successfully when value is valid")]
    public void ApplyDiscount_ShouldApplySuccessfully_WhenValueIsValid()
    {
        // arrange
        var orderItem = CreateValidItem(price: 200m, quantity: 2);
        // act
        orderItem.ApplyDiscount(50m);
        // assert
        orderItem.AppliedDiscount.Should().Be(50m);
        orderItem.TotalAmount.Should().Be(350m); // (200 * 2) - 50
        orderItem.UpdatedAt.Should().NotBeNull();
    }

    [Theory(DisplayName = "Should throw DomainException on apply invalid discound")]
    [InlineData(-10, "Discount cannot be negative.")]
    [InlineData(1000, "Discount cannot exceed the total.")]
    public void ApplyDiscount_ShouldThrowDomainException_WhenValueIsInvalid
    (
        decimal discount,
        string message
    )
    {
        // arrange
        OrderItem orderItem = CreateValidItem(price: 100m, quantity: 2);
        // act
        Action act = () => orderItem.ApplyDiscount(discount);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage(message);
    }

    // Add or Remove Unit Tests

    [Fact(DisplayName = "Should add units successfully when value is valid")]
    public void AddUnits_ShouldAddSuccessfully_WhenValueIsValid()
    {
        // arrange
        OrderItem orderItem = CreateValidItem(price: 50m, quantity: 2);
        // act
        orderItem.AddUnits(3);
        // assert
        orderItem.Quantity.Should().Be(5);
        orderItem.TotalAmount.Should().Be(250m);
        orderItem.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Should throw DomainException on add invalid units")]
    public void AddUnits_ShouldThrowDomainException_WhenValueIsInvalid()
    {
        // arrange
        OrderItem orderItem = CreateValidItem();
        // act
        Action act = () => orderItem.AddUnits(0);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Units should be added at least one or more.");
    }

    [Fact(DisplayName = "Should remove units successfully when value is valid")]
    public void RemoveUnits_ShouldRemoveSuccessfully_WhenValueIsValid()
    {
        // arrange
        OrderItem orderItem = CreateValidItem(price: 100m, quantity: 5);
        // act
        orderItem.RemoveUnits(2);
        // assert
        orderItem.Quantity.Should().Be(3);
        orderItem.TotalAmount.Should().Be(300m);
        orderItem.UpdatedAt.Should().NotBeNull();
    }

    [Theory(DisplayName = "Should throw DomainException on remove invalid units")]
    [InlineData(0, "Should remove at least one unit.")]
    [InlineData(10, "Cannot remove more units than are available in the item.")]
    public void RemoveUnits_ShoudThrowDomainException_WhenValueIsInvalid
    (
        int units,
        string message
    )
    {
        // arrange
        OrderItem orderItem = CreateValidItem(price: 100m, quantity: 3);
        // act
        Action act = () => orderItem.RemoveUnits(units);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage(message);
    }

    [Fact(DisplayName = "Should throw DomainException on remove units and quantity comes to be zero")]
    public void RemoveUnits_ShouldThrowDomainException_WhenQuantityComesToBeZero()
    {
        // arrange
        OrderItem orderItem = CreateValidItem(price: 100m, quantity: 2);
        // act
        Action act = () => orderItem.RemoveUnits(2);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("An order item cannot have zero quantity. (Use the method from Order class to remove it)");
    }

    // Update price tests

    [Fact(DisplayName = "Should update unit price successfully when value is valid")]
    public void UpdateUnitPrice_ShouldUpdateSuccessfully_WhenValueIsValid()
    {
        // arrange
        OrderItem orderItem = CreateValidItem(price: 100m, quantity: 3);
        // act
        orderItem.UpdateUnitPrice(150m);
        // assert
        orderItem.UnitPrice.Should().Be(150m);
        orderItem.TotalAmount.Should().Be(450m);
        orderItem.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Should throw DomainException on update invalid unit price")]
    public void UpdateUnitPrice_ShouldThrowDomainException_WhenValueIsInvalid()
    {
        // arrange
        OrderItem orderItem = CreateValidItem();
        // act
        Action act = () => orderItem.UpdateUnitPrice(0);
        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("The unit price should be greater then zero.");
    }

    // OrderItem equality tests

    [Fact(DisplayName = "Two order item with the same id should be considered equals")]
    public void Equals_ShouldREturnTrue_WhenIdAreTheSame()
    {
        // arrange
        OrderItem orderItem1 = CreateValidItem();
        OrderItem orderItem2 = CreateValidItem();

        // force the same id using reflection
        typeof(Entity).GetProperty("Id")!.SetValue(orderItem2, orderItem1.Id);

        // act & assert
        (orderItem1 == orderItem2)
                  .Should()
                  .BeTrue();

        orderItem1.Equals(orderItem2)
                  .Should()
                  .BeTrue();        
    }
}