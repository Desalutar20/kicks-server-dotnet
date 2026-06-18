using Domain.Orders;
using FluentAssertions;

namespace Unit.Order;

public class OrderAddressTests
{
    [Theory]
    [InlineData("New York", "5th Avenue", "350", "12B")]
    [InlineData("Los Angeles", "Sunset Boulevard", "1024", "Apt 3")]
    [InlineData("San Francisco", "Market Street", "1", "Unit 5")]
    [InlineData("Chicago", "Michigan Avenue", "200", "10A")]
    [InlineData("Seattle", "Pine Street", "88", "4")]
    [InlineData("Austin", "Congress Avenue", "500", "Suite 2")]
    public void Create_ValidUsAddresses_ReturnsSuccess(
        string city,
        string street,
        string home,
        string apartment
    )
    {
        var result = OrderAddress.Create(city, street, home, apartment);

        result.IsSuccess.Should().BeTrue();
        result.Value.City.Should().Be(city);
        result.Value.Street.Should().Be(street);
        result.Value.Home.Should().Be(home);
        result.Value.Apartment.Should().Be(apartment);
    }

    [Fact]
    public void Create_TrimsInputs_ReturnsTrimmedValues()
    {
        var result = OrderAddress.Create("  New York  ", "  5th Avenue  ", "  350  ", "  12B  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.City.Should().Be("New York");
        result.Value.Street.Should().Be("5th Avenue");
        result.Value.Home.Should().Be("350");
        result.Value.Apartment.Should().Be("12B");
    }

    [Theory]
    [InlineData("", "5th Avenue", "350", "12B")]
    [InlineData("New York", "", "350", "12B")]
    [InlineData("New York", "5th Avenue", "", "12B")]
    [InlineData("New York", "5th Avenue", "350", "")]
    public void Create_EmptyFields_ReturnsFailure(
        string city,
        string street,
        string home,
        string apartment
    )
    {
        var result = OrderAddress.Create(city, street, home, apartment);

        result.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData("   ", "5th Avenue", "350", "12B")]
    [InlineData("New York", "   ", "350", "12B")]
    [InlineData("New York", "5th Avenue", "   ", "12B")]
    [InlineData("New York", "5th Avenue", "350", "   ")]
    public void Create_WhitespaceOnlyFields_ReturnsFailure(
        string city,
        string street,
        string home,
        string apartment
    )
    {
        var result = OrderAddress.Create(city, street, home, apartment);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_CityExceedsMaxLength_ReturnsFailure()
    {
        var city = new string('a', OrderAddress.MaxCityLength + 1);

        var result = OrderAddress.Create(city, "5th Avenue", "350", "12B");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_StreetExceedsMaxLength_ReturnsFailure()
    {
        var street = new string('a', OrderAddress.MaxStreetLength + 1);

        var result = OrderAddress.Create("New York", street, "350", "12B");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_HomeExceedsMaxLength_ReturnsFailure()
    {
        var home = new string('a', OrderAddress.MaxHomeLength + 1);

        var result = OrderAddress.Create("New York", "5th Avenue", home, "12B");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ApartmentExceedsMaxLength_ReturnsFailure()
    {
        var apartment = new string('a', OrderAddress.MaxApartmentLength + 1);

        var result = OrderAddress.Create("New York", "5th Avenue", "350", apartment);

        result.IsFailure.Should().BeTrue();
    }
}
