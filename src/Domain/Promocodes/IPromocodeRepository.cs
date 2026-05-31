using Domain.Shared;
using Domain.Shared.Pagination;

namespace Domain.Promocodes;

public interface IPromocodeRepository
{
    Task<KeysetPaginated<Promocode, PromocodeId>> GetPromocodesAsync(
        PromocodeCode? code,
        KeysetPagination<PromocodeId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<Promocode?> GetPromocodeByIdAsync(
        PromocodeId id,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<Promocode?> GetPromocodeByCodeAsync(
        PromocodeCode code,
        bool trackChanges,
        CancellationToken ct = default
    );

    void CreatePromocode(Promocode promocode);
    void UpdatePromocode(Promocode promocode);
    void DeletePromocode(Promocode promocode);
}
