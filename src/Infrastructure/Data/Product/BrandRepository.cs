using Domain.Brand;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Product;

internal sealed class BrandRepository(AppDbContext dbContext)
    : RepositoryBase<Brand>(dbContext),
        IBrandRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public void CreateBrand(Brand brand) => Create(brand);

    public void DeleteBrand(Brand brand) => Delete(brand);

    public async Task<KeysetPaginated<Brand, BrandId>> GetBrandsAsync(
        NonEmptyString? search,
        KeysetPagination<BrandId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.Brands.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        var searchTerm = search?.Value.ToLower();

        query = query
            .WhereNotNull(searchTerm, u => ((string)u.Name).ToLower().StartsWith(searchTerm!))
            .ApplyKeysetPagination(keysetPagination);

        var result = await query.ToListAsync(ct);

        return new KeysetPaginated<Brand, BrandId>(
            result,
            keysetPagination,
            u => u.CreatedAt,
            u => u.Id
        );
    }

    public async Task<Brand?> GetBrandByNameAsync(
        BrandName name,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(x => x.Name == name, trackChanges).FirstOrDefaultAsync(ct);

    public async Task<Brand?> GetBrandByIdAsync(
        BrandId id,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync(ct);

    public void UpdateBrand(Brand brand) => Update(brand);
}
