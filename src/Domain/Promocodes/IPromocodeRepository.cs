using Domain.Shared.Pagination;
using Domain.Shared.ValueObjects;

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

    Task<IEnumerable<PromocodeId>> GetAndLockInvalidPromocodeIdsAsync(
        PositiveInt batchSize,
        CancellationToken ct = default
    );

    Task BulkDecrementUsageCountAsync(
        IReadOnlyCollection<PromocodeId> ids,
        CancellationToken ct = default
    );

    void CreatePromocode(Promocode promocode);
    void UpdatePromocode(Promocode promocode);
    void DeletePromocode(Promocode promocode);
}
