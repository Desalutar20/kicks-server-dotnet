using Domain.Promocodes;

namespace Application.Admin.Promocodes.Types;

public sealed record AdminPromocodeResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public int DiscountValue { get; init; }
    public required string Type { get; init; }
    public DateTimeOffset ValidFrom { get; init; }
    public DateTimeOffset ValidTo { get; init; }
    public int UsageLimit { get; init; }
    public int UsageCount { get; init; }
    public required string Code { get; init; }
}

internal static class AdminPromocodeResponseMapper
{
    public static AdminPromocodeResponse ToAdminResponse(this Promocode promocode)
    {
        return new AdminPromocodeResponse()
        {
            Id = promocode.Id.Value,
            CreatedAt = promocode.CreatedAt,
            UpdatedAt = promocode.UpdatedAt,
            DiscountValue = promocode.DiscountValue,
            Type = promocode.Type.ToString(),
            ValidFrom = promocode.ValidityPeriod.ValidFrom,
            ValidTo = promocode.ValidityPeriod.ValidTo,
            UsageLimit = promocode.UsageLimit,
            UsageCount = promocode.UsageCount,
            Code = promocode.Code.Value,
        };
    }
}
