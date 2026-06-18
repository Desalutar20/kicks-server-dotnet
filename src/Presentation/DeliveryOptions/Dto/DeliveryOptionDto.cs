using Domain.DeliveryOptions;

namespace Presentation.DeliveryOptions.Dto;

public sealed record DeliveryOptionDto(Guid Id, string Title, string Description, decimal Price);

internal static class DeliveryOptionDtoMapper
{
    public static DeliveryOptionDto ToDto(this DeliveryOption deliveryOption) =>
        new(
            deliveryOption.Id.Value,
            deliveryOption.Title.Value,
            deliveryOption.Description.Value,
            deliveryOption.Price.Dollars
        );
}
