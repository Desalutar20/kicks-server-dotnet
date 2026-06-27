using Domain.Promocodes;

namespace Presentation.Cart.Dto;

public sealed record PromocodeDto(
    int DiscountValue,
    PromocodeType Type,
    DateTimeOffset ValidTo,
    string Code
);

internal static class PromocodeDtoMapper
{
    public static PromocodeDto ToDto(this Promocode promocode) =>
        new(
            promocode.DiscountValue.Value,
            promocode.Type,
            promocode.ValidityPeriod.ValidTo,
            promocode.Code.Value
        );
}
