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
            postalCode: "12345678",
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
        address.PostalCode.Should().Be("12345678");
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
    public void Should_Throw_Error_WhenPostalCodeIsIvalid(string? postalCode)
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

    [Fact]
    public void Should_ThrowError_WhenPostalCodeDoesntHave8Digits()
    {
        // arrange
        var act = () => new Address
        (
            postalCode: "1234",
            street: "Rua A",
            neighborhood: "Centro",
            state: "SP",
            city: "Sao Paulo",
            country: "Brasil"
        );

        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Invalid postal code format.");
    }

    [Theory]
    [InlineData(null, "Centro", "Sao Paulo", "SP", "Brasil")]
    [InlineData("Rua A", null, "Sao Paulo", "SP", "Brasil")]
    [InlineData("Rua A", "Centro", null, "SP", "Brasil")]
    [InlineData("Rua A", "Centro", "Sao Paulo", null, "Brasil")]
    [InlineData("Rua A", "Centro", "Sao Paulo", "SP", null)]
    public void Should_ThrowError_WhenRequiredFieldsAreInvalid
    (
        string? street,
        string? neighborhood,
        string? city,
        string? state,
        string? country
    )
    {
        // arrange
        var act = () => new Address
        (
            postalCode: "12345678",
            street: street!,
            neighborhood: neighborhood!,
            state: state!,
            city: city!,
            country: country!
        );

        // assert
        act.Should()
           .Throw<DomainException>();
    }

    [Fact]
    public void Should_UpdateAddress_With_ValidData()
    {
        // arrange
        var address = CreateValidAddress();

        // act
        address.Update
        (
            postalCode: "87654321",
            street: "Rua B",
            neighborhood: "Bairro Novo",
            state: "RJ",
            city: "Rio de Janeiro",
            country: "Brasil",
            complement: "Apto 12"
        );

        // assert
        address.PostalCode.Should().Be("87654321");
        address.Street.Should().Be("Rua B");
        address.Neighborhood.Should().Be("Bairro Novo");
        address.State.Should().Be("RJ");
        address.City.Should().Be("Rio de Janeiro");
        address.Country.Should().Be("Brasil");
        address.Complement.Should().Be("Apto 12");
    }

    [Fact]
    public void Should_ThrowError_WhenUpdatePostalCode_WithInvalidData()
    {
        // arrange
        var address = CreateValidAddress();

        // act
        var act = () => address.Update
        (
            postalCode: "123",
            street: "Rua B",
            neighborhood: "Bairro Novo",
            state: "RJ",
            city: "Rio de Janeiro",
            country: "Brasil",
            complement: "Apto 12"
        );

        // assert
        act.Should()
           .Throw<DomainException>()
           .WithMessage("Invalid postal code format.");
    }

    [Fact]
    public void Should_ThrowError_OnUpdateWithInvalidRequiredData()
    {
        // arrange
        var address = CreateValidAddress();

        // act
        var act = () => address.Update
        (
            postalCode: "87654321",
            street: "",
            neighborhood: "Center",
            state: "SP",
            city: "SP",
            country: "Brasil"
        );

        // assert
        act.Should()
           .Throw<DomainException>();
    }
}
