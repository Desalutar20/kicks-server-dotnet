using Application.Admin.Promocodes.Types;
using Domain.Promocodes;

namespace Application.Admin.Promocodes;

public interface IPromocodeReadRepository
{
    Task<KeysetPaginated<AdminPromocodeResponse, Guid>> GetPromocodesAsync(
        PromocodeCode? code,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    );
}
