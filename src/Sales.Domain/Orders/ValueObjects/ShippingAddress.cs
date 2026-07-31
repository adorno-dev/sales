using System.Text.RegularExpressions;
using Sales.Domain.Common.Base;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.Common.Validations;

namespace Sales.Domain.Orders.ValueObjects;

public sealed class ShippingAddress : ValueObject
{
    public string PostalCode { get; private set; }
    public string Street { get; private set; }
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; }
    public string State { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    private ShippingAddress(
        string postalCode,
        string street,
        string? complement,
        string neighborhood,
        string state,
        string city,
        string country)
    {
        Guard.AgainstNullOrWhiteSpace(postalCode, nameof(PostalCode));
        Guard.AgainstNullOrWhiteSpace(street, nameof(Street));
        Guard.AgainstNullOrWhiteSpace(neighborhood, nameof(Neighborhood));
        Guard.AgainstNullOrWhiteSpace(state, nameof(State));
        Guard.AgainstNullOrWhiteSpace(city, nameof(City));
        Guard.AgainstNullOrWhiteSpace(country, nameof(Country));

        if (!Regex.IsMatch(postalCode ?? "", @"^\d{5}-\d{3}$"))
            throw new DomainException(
                "Invalid postal code. Expected format: 00000-000.");

        PostalCode = postalCode!.Trim();
        Street = street.Trim();
        Complement = string.IsNullOrWhiteSpace(complement) 
            ? null 
            : complement.Trim();
        Neighborhood = neighborhood.Trim();
        State = state.Trim();
        City = city.Trim();
        Country = country.Trim();
    }

    public static ShippingAddress Create(
        string postalCode,
        string street,
        string? complement,
        string neighborhood,
        string state,
        string city,
        string country)
        => new(postalCode, street, complement, neighborhood, state, city, country);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PostalCode;
        yield return Street;
        yield return Complement ?? string.Empty;
        yield return Neighborhood;
        yield return State;
        yield return City;
        yield return Country;
    }

    public override string ToString()
        => $"{Street}, {Complement} - {Neighborhood}, {City} - {State}, {Country} - Postal Code: {PostalCode}";
}