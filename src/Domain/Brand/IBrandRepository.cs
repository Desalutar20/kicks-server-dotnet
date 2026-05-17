using Domain.Shared;
using Domain.Shared.Pagination;

namespace Domain.Brand;

public interface IBrandRepository
{
    Task<KeysetPaginated<Brand, BrandId>> GetBrandsAsync(
        NonEmptyString? search,
        KeysetPagination<BrandId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<Brand?> GetBrandByNameAsync(
        BrandName name,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<Brand?> GetBrandByIdAsync(BrandId id, bool trackChanges, CancellationToken ct = default);

    void CreateBrand(Brand brand);
    void UpdateBrand(Brand brand);
    void DeleteBrand(Brand brand);
}
