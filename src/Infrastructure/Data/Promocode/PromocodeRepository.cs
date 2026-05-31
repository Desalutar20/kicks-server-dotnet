using Domain.Promocodes;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Promocode;

internal sealed class PromocodeRepository(AppDbContext dbContext)
    : RepositoryBase<DomainPromocode>(dbContext),
        IPromocodeRepository
{
    private readonly AppDbContext _dbContext = dbContext;

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
