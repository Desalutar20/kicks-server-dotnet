using Domain.DeliveryOptions;

namespace Presentation.Admin.DeliveryOptions.Dto;

public sealed record AdminDeliveryOptionDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Title,
    string Description,
    decimal Price
);

internal static class AdminDeliveryOptionDtoMapper
{
    public static AdminDeliveryOptionDto ToDto(this DeliveryOption deliveryOption) =>
        new(
            deliveryOption.Id.Value,
            deliveryOption.CreatedAt,
            deliveryOption.UpdatedAt,
            deliveryOption.Title.Value,
            deliveryOption.Description.Value,
            deliveryOption.Price.Dollars
        );
}
