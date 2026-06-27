using Application.Admin.Promocodes.Types;
using Domain.Promocodes;

namespace Application.Admin.Promocodes.UseCases.GetPromocodes;

public sealed record GetPromocodesQuery(
    PromocodeCode? Code,
    KeysetPagination<Guid> KeysetPagination
) : IQuery<KeysetPaginated<AdminPromocodeResponse, Guid>>;

internal sealed class GetPromocodesQueryHandler(IPromocodeReadRepository promocodeReadRepository)
    : IQueryHandler<GetPromocodesQuery, KeysetPaginated<AdminPromocodeResponse, Guid>>
{
    public async Task<Result<KeysetPaginated<AdminPromocodeResponse, Guid>>> Handle(
        GetPromocodesQuery query,
        CancellationToken ct = default
    )
    {
        var data = await promocodeReadRepository.GetPromocodesAsync(
            query.Code,
            query.KeysetPagination,
            ct
        );

        return data;
    }
}
