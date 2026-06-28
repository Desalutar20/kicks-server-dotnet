using Domain.DeliveryOptions;

namespace Application.Admin.DeliveryOptions.Types;

public sealed record AdminDeliveryOptionResponse(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Title,
    string Description,
    decimal Price
);

public static class AdminDeliveryOptionResponseMapper
{
    public static DeliveryOptionResponse ToDeliveryOptionResponse(
        this AdminDeliveryOptionResponse option
    )
    {
        return new DeliveryOptionResponse
        {
            Id = option.Id,
            Title = option.Title,
            Description = option.Description,
            Price = option.Price,
        };
    }

    public static AdminDeliveryOptionResponse ToAdminResponse(this DeliveryOption option)
    {
        return new AdminDeliveryOptionResponse(
            option.Id.Value,
            option.CreatedAt,
            option.UpdatedAt,
            option.Title.Value,
            option.Description.Value,
            option.Price.Dollars
        );
    }
}
