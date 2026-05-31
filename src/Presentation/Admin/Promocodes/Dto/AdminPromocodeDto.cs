using Domain.Promocodes;

namespace Presentation.Admin.Promocodes.Dto;

public sealed record AdminPromocodeDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int DiscountValue,
    PromocodeType Type,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    int UsageLimit,
    int UsageCount,
    string Code
);

internal static class AdminPromocodeDtoMapper
{
    public static AdminPromocodeDto ToDto(this Promocode promocode) =>
        new(
            promocode.Id.Value,
            promocode.CreatedAt,
            promocode.UpdatedAt,
            promocode.DiscountValue.Value,
            promocode.Type,
            promocode.ValidityPeriod.ValidFrom,
            promocode.ValidityPeriod.ValidTo,
            promocode.UsageLimit.Value,
            promocode.UsageCount,
            promocode.Code.Value
        );
}
