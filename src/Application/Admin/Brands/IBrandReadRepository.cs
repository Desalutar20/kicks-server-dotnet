using Application.Admin.Brands.Types;
using Domain.Shared.ValueObjects;

namespace Application.Admin.Brands;

public interface IBrandReadRepository
{
    Task<KeysetPaginated<AdminBrandResponse, Guid>> GetBrandsAsync(
        NonEmptyString? search,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    );
}
