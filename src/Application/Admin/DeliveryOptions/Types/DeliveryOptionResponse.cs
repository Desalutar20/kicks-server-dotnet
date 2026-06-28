using Domain.DeliveryOptions;

namespace Application.Admin.DeliveryOptions.Types;

public sealed record DeliveryOptionResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public decimal Price { get; init; }
}

internal static class DeliveryOptionResponseMapper
{
    public static DeliveryOptionResponse ToResponse(this DeliveryOption option)
    {
        return new DeliveryOptionResponse
        {
            Id = option.Id.Value,
            Title = option.Title.Value,
            Description = option.Description.Value,
            Price = option.Price.Dollars,
        };
    }
}
