namespace Application.Orders.Types;

public sealed record OrderAddressResponse
{
    public required string City { get; init; }
    public required string Street { get; init; }
    public required string Home { get; init; }
    public required string Apartment { get; init; }
}
