namespace Application.Admin.Brands.UseCases.GetBrands;

public sealed record GetBrandsQuery(
    NonEmptyString? Search,
    KeysetPagination<BrandId> KeysetPagination
) : IQuery<KeysetPaginated<Brand, BrandId>>;

internal sealed class GetBrandsQueryHandler(IBrandRepository brandRepository)
    : IQueryHandler<GetBrandsQuery, KeysetPaginated<Brand, BrandId>>
{
    public async Task<Result<KeysetPaginated<Brand, BrandId>>> Handle(
        GetBrandsQuery query,
        CancellationToken ct = default
    )
    {
        var data = await brandRepository.GetBrandsAsync(
            query.Search,
            query.KeysetPagination,
            false,
            ct
        );

        return Result<KeysetPaginated<Brand, BrandId>>.Success(data);
    }
}
