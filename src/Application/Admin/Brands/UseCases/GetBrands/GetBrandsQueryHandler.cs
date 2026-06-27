using Application.Admin.Brands.Types;
using Domain.Shared.ValueObjects;

namespace Application.Admin.Brands.UseCases.GetBrands;

public sealed record GetBrandsQuery(NonEmptyString? Search, KeysetPagination<Guid> KeysetPagination)
    : IQuery<KeysetPaginated<AdminBrandResponse, Guid>>;

internal sealed class GetBrandsQueryHandler(IBrandReadRepository brandReadRepository)
    : IQueryHandler<GetBrandsQuery, KeysetPaginated<AdminBrandResponse, Guid>>
{
    public async Task<Result<KeysetPaginated<AdminBrandResponse, Guid>>> Handle(
        GetBrandsQuery query,
        CancellationToken ct = default
    )
    {
        var data = await brandReadRepository.GetBrandsAsync(
            query.Search,
            query.KeysetPagination,
            ct
        );

        return data;
    }
}
