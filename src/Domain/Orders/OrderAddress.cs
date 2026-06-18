using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Orders;

public sealed record OrderAddress
{
    public const int MaxCityLength = 100;
    public const int MaxStreetLength = 100;
    public const int MaxHomeLength = 20;
    public const int MaxApartmentLength = 20;

    public string City { get; } = null!;
    public string Street { get; } = null!;
    public string Home { get; } = null!;
    public string Apartment { get; } = null!;

    private OrderAddress(string city, string street, string home, string apartment)
    {
        City = city;
        Street = street;
        Home = home;
        Apartment = apartment;
    }

    public static Result<OrderAddress> Create(
        string city,
        string street,
        string home,
        string apartment,
        string field = "address"
    )
    {
        var errors = new List<string>();

        if (Guard.AgainstEmptyString(city).IsFailure)
            errors.Add("City cannot be empty");

        if (Guard.AgainstEmptyString(street).IsFailure)
            errors.Add("Street cannot be empty");

        if (Guard.AgainstEmptyString(home).IsFailure)
            errors.Add("Home cannot be empty");

        if (Guard.AgainstEmptyString(apartment).IsFailure)
            errors.Add("Apartment cannot be empty");

        if (errors.Count > 0)
        {
            return Error.Validation(field, errors);
        }

        city = city.Trim();
        street = street.Trim();
        home = home.Trim();
        apartment = apartment.Trim();

        var cityLengthResult = Guard.ForStringLength(city, 1, MaxCityLength, "City");
        if (cityLengthResult.IsFailure)
        {
            errors.Add(cityLengthResult.Error.Description);
        }

        var streetLengthResult = Guard.ForStringLength(street, 1, MaxStreetLength, "Street");
        if (streetLengthResult.IsFailure)
        {
            errors.Add(streetLengthResult.Error.Description);
        }

        var homeLengthResult = Guard.ForStringLength(home, 1, MaxHomeLength, "Home");
        if (homeLengthResult.IsFailure)
        {
            errors.Add(homeLengthResult.Error.Description);
        }

        var apartmentLengthResult = Guard.ForStringLength(
            apartment,
            1,
            MaxApartmentLength,
            "Apartment"
        );
        if (apartmentLengthResult.IsFailure)
        {
            errors.Add(apartmentLengthResult.Error.Description);
        }

        return errors.Count == 0
            ? new OrderAddress(city, street, home, apartment)
            : Error.Validation(field, errors);
    }
}
