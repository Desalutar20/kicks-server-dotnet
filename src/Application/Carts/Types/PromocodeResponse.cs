using Domain.Promocodes;

namespace Application.Carts.Types;

public sealed record PromocodeResponse
{
    public int DiscountValue { get; init; }
    public PromocodeType Type { get; init; }
    public DateTimeOffset ValidTo { get; init; }
    public required string Code { get; init; }
}
