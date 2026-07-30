using FluentAssertions;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Tests.ValueObjects;

public class ShippingAddressTests
{
    [Fact(DisplayName = "Should create a ShippingAdress successfully when all data are valid.")]
    public void Create_ShouldReturnValidAddress_WhenDataIsValid()
    {
        // arrange
        var arrange = new 
        {
            PostalCode = "12345-678",
            Street = "Rua das Flores",
            Complement = "Apto 101",
            Neighborhood = "Centro",
            State = "SP",
            City = "Sao Paulo",
            Country = "Brasil"
        };

        // act
        var shippingAddress = ShippingAddress.Create
        (
            arrange.PostalCode,
            arrange.Street,
            arrange.Complement,
            arrange.Neighborhood,
            arrange.State,
            arrange.City,
            arrange.Country
        );

        // assert
        shippingAddress.Should().NotBeNull();
        shippingAddress.PostalCode.Should().Be(arrange.PostalCode);
        shippingAddress.Street.Should().Be(arrange.Street);
        shippingAddress.Complement.Should().Be(arrange.Complement);
        shippingAddress.ToString().Contains("Rua das Flores");
    }

    [Theory(DisplayName = "Should throw DomainException when postalcode is invalid.")]
    [InlineData("12345678")]
    [InlineData("12-345678")]
    [InlineData("ABCDE-123")]
    public void Create_ShouldThrowDomainException_WhenPostalCodeIsInvalid(string postalCode)
    {
        // arrange
        var arrange = new 
        {
            Street = "Rua das Flores",
            Complement = "Apto 101",
            Neighborhood = "Centro",
            State = "SP",
            City = "Sao Paulo",
            Country = "Brasil"
        };

        // act
        Action act = () => ShippingAddress.Create
        (
            postalCode,
            arrange.Street,
            arrange.Complement,
            arrange.Neighborhood,
            arrange.State,
            arrange.City,
            arrange.Country
        );

        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Invalid postal code.*");
    }

    [Fact(DisplayName = "Two addresses with the same data should be equals (Value Object)")]
    public void ShippingAddressShouldBeEquals_WhenDataAreSame()
    {
        // arrange
        var address1 = ShippingAddress.Create("12345-678", "Rua X", "Apto 101", "Centro", "SP", "Sao Paulo", "Brasil");
        var address2 = ShippingAddress.Create("12345-678", "Rua X", "Apto 101", "Centro", "SP", "Sao Paulo", "Brasil");

        // assert
        address1.Should()
                .Be(address2);

        (address1 == address2)
                .Should()
                .BeTrue();
    }

    [Fact(DisplayName = "Two addresses with some field different should be different (Value Object)")]
    public void ShippingAddressShouldBeDifferent_WhenSomeFieldIsDifferent()
    {
        // arrange
        var address1 = ShippingAddress.Create("12345-678", "Rua X", "Apto 101", "Centro", "SP", "Sao Paulo", "Brasil");
        var address2 = ShippingAddress.Create("12345-678", "Rua Y", "Apto 101", "Centro", "SP", "Sao Paulo", "Brasil");

        // assert
        address1.Should()
                .NotBe(address2);
    }

    [Fact(DisplayName = "ShippingAddress should be imuttable after creation")]
    public void ShippingAddressShouldBeImuttable_AfterCreation()
    {
        // arrange
        var address = ShippingAddress.Create("12345-678", "Rua X", "Apto 101", "Centro", "SP", "Sao Paulo", "Brasil");

        // act
        Action act = () =>
        { /* Some uncommon (don't compile, only concept) - address.PostalCode = "99999-999" */ };

        // assert
        address.GetType()
               .GetProperties()
               .All(p => p.SetMethod == null || p.SetMethod.IsPrivate)
               .Should()
               .BeTrue("The VO properties should be immutable.");
    }

    [Theory(DisplayName = "Should throw DomainException when required fields are null or empty")]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
    [InlineData(null, "Street", "Neighborhood", "State", "City", "Country")]        // null postalcode
    [InlineData("12345-678", null, "Neighborhood", "State", "City", "Country")]     // null street
    [InlineData("12345-678", "Street", "Neighborhood", "State", "City", null)]      // null country
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
    public void Create_ShouldThrowDomainException_WhenRequiredFieldsAreNullOrEmpty
    (
        string postalCode,
        string street,
        string neighborhood,
        string state,
        string city,
        string country
    )
    {
        // act
        Action act = () => ShippingAddress.Create
        (
            postalCode,
            street,
            "complement",
            neighborhood,
            state,
            city,
            country
        );

        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("*cannot be null or empty*");
    }
}