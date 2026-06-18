using Domain.Shared;
using Domain.Shared.Pagination;
using Domain.Shared.ValueObjects;

namespace Domain.Brands;

public interface IBrandRepository
{
    Task<KeysetPaginated<Brand, BrandId>> GetBrandsAsync(
        NonEmptyString? search,
        KeysetPagination<BrandId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<Brand?> GetBrandByIdAsync(BrandId id, bool trackChanges, CancellationToken ct = default);

    void CreateBrand(Brand brand);
    void UpdateBrand(Brand brand);
    void DeleteBrand(Brand brand);
}
