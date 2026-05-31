using Domain.Promocodes;

namespace Application.Admin.Promocodes.UseCases.GetPromocodes;

public sealed record GetPromocodesQuery(
    PromocodeCode? Code,
    KeysetPagination<PromocodeId> KeysetPagination
) : IQuery<KeysetPaginated<Promocode, PromocodeId>>;

internal sealed class GetPromocodesQueryHandler(IPromocodeRepository promocodeRepository)
    : IQueryHandler<GetPromocodesQuery, KeysetPaginated<Promocode, PromocodeId>>
{
    public async Task<Result<KeysetPaginated<Promocode, PromocodeId>>> Handle(
        GetPromocodesQuery query,
        CancellationToken ct = default
    )
    {
        var data = await promocodeRepository.GetPromocodesAsync(
            query.Code,
            query.KeysetPagination,
            false,
            ct
        );

        return data;
    }
}
