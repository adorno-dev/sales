using FluentAssertions;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.Customers.Entities;

namespace Sales.Domain.Tests.Customers.Entities;

public class AdressTests
{
    private static Address CreateValidAddress()
    {
        return new Address
        (
            postalCode: "12345-678",
            city: "Los Angeles",
            street: "Main Street",
            neighborhood: "Main Neighborhood",
            state: "CA",
            country: "United States"
        );
    }

    [Fact]
    public void Should_Create_Valid_Address()
    {
        // arrange & act
        var address = CreateValidAddress();
        // assert
        address.PostalCode.Should().Be("12345-678");
        address.Street.Should().Be("Main Street");
        address.Neighborhood.Should().Be("Main Neighborhood");
        address.State.Should().Be("CA");
        address.City.Should().Be("Los Angeles");
        address.Country.Should().Be("United States");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_Exception_When_PostalCode_Is_Invalid(string? postalCode)
    {
        // arrange
        Action act = () => new Address
        (
            postalCode: postalCode!,
            city: "Los Angeles",
            street: "Main Street",
            neighborhood: "Main Neighborhood",
            state: "CA",
            country: "United States"
        );

        // assert
        act.Should().Throw<DomainException>()
           .WithMessage("Postal code is required.");
    }
}
