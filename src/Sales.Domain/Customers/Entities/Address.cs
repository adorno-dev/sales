using Sales.Domain.Common.Base;
using Sales.Domain.Common.Exceptions;
using Sales.Domain.Common.Validations;
using System.Text.RegularExpressions;

namespace Sales.Domain.Customers.Entities;

public sealed class Address : Entity
{
    public string PostalCode { get; private set; }
    public string Street { get; private set; }
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; }
    public string State { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    public Address
    (
        string postalCode,
        string street,
        string neighborhood,
        string state,
        string city,
        string country,
        string? complement = null
)
    {
        Validate(postalCode, street, neighborhood, city, state, country);

        PostalCode = postalCode;
        Street = street;
        Complement = complement;
        Neighborhood = neighborhood;
        State = state;
        City = city;
        Country = country;
    }

    internal void Update
    (
        string postalCode,
        string street,
        string neighborhood,
        string state,
        string city,
        string country,
        string? complement = null
    )
    {
        Validate(postalCode, street, neighborhood, city, state, country);

        PostalCode = postalCode;
        Street = street;
        Neighborhood = neighborhood;
        State = state;
        City = city;
        Country = country;
        Complement = complement;
    }

    private void Validate
    (
        string postalCode,
        string street,
        string neighborhood,
        string city,
        string state,
        string country
    )
    {
        Guard.AgainstNullOrWhiteSpace(postalCode, nameof(postalCode), "Postal code is required.");
        Guard.Against<DomainException>(!Regex.IsMatch(postalCode, @"^\d{8}$"), "Invalid postal code format.");
        Guard.AgainstNullOrWhiteSpace(street, nameof(street), "Street is required.");
        Guard.Against<DomainException>(street.Length < 3, "Street must be at least 3 characters long.");
        Guard.AgainstNullOrWhiteSpace(neighborhood, nameof(neighborhood), "Neighborhood is required.");
        Guard.AgainstNullOrWhiteSpace(state, nameof(state), "State is required.");
        Guard.AgainstNullOrWhiteSpace(city, nameof(city), "City is required.");
        Guard.AgainstNullOrWhiteSpace(country, nameof(country), "Country is required.");
    }
}