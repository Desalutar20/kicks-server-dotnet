using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.DeliveryOptions.Types;

public sealed record DeliveryOptionResponse(
    DeliveryOptionId Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DeliveryOptionTitle Title,
    DeliveryOptionDescription Description,
    Money Price
);

internal static class DeliveryOptionResponseMapper
{
    public static DeliveryOptionResponse ToDto(this DeliveryOption option)
    {
        DeliveryOptionDescription deliveryOptionDescription = option.Description;
        return new(
            option.Id,
            option.CreatedAt,
            option.UpdatedAt,
            option.Title,
            option.Description,
            option.Price
        );
    }
}
