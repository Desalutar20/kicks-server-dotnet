using Domain.Promocodes;
using Domain.Shared.ValueObjects;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Promocode;

internal sealed class PromocodeRepository(AppDbContext dbContext)
    : RepositoryBase<DomainPromocode>(dbContext),
        IPromocodeRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<PromocodeId>> GetAndLockInvalidPromocodeIdsAsync(
        PositiveInt batchSize,
        CancellationToken ct = default
    )
    {
        return await _dbContext
            .Promocodes.FromSqlInterpolated(
                $"""
                    SELECT id
                    FROM promocode
                    WHERE NOW() > valid_to
                      OR usage_count >= usage_limit
                    ORDER BY created_at
                    FOR UPDATE SKIP LOCKED
                    LIMIT {batchSize.Value}
                """
            )
            .Select(x => x.Id)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task BulkDecrementUsageCountAsync(
        IReadOnlyCollection<PromocodeId> ids,
        CancellationToken ct = default
    )
    {
        await _dbContext
            .Promocodes.Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UsageCount, x => x.UsageCount - 1), ct);
    }

    public void CreatePromocode(DomainPromocode promocode) => Create(promocode);

    public void DeletePromocode(DomainPromocode promocode) => Delete(promocode);

    public async Task<KeysetPaginated<DomainPromocode, PromocodeId>> GetPromocodesAsync(
        PromocodeCode? code,
        KeysetPagination<PromocodeId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.Promocodes.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        query = query
            .WhereNotNull(code, u => u.Code == code)
            .ApplyKeysetPagination(keysetPagination);

        var result = await query.ToListAsync(ct);

        return new KeysetPaginated<DomainPromocode, PromocodeId>(
            result,
            keysetPagination,
            u => u.CreatedAt,
            u => u.Id
        );
    }

    public async Task<DomainPromocode?> GetPromocodeByIdAsync(
        PromocodeId id,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync(ct);

    public async Task<DomainPromocode?> GetPromocodeByCodeAsync(
        PromocodeCode code,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(x => x.Code == code, trackChanges).FirstOrDefaultAsync(ct);

    public void UpdatePromocode(DomainPromocode promocode) => Update(promocode);
}
