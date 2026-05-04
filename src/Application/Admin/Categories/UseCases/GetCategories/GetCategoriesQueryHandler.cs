using Domain.Product.Category;
using Domain.Shared.Pagination;

namespace Application.Admin.Categories.UseCases.GetCategories;

public sealed record GetCategoriesQuery(
    NonEmptyString? Search,
    KeysetPagination<CategoryId> KeysetPagination
) : IQuery<KeysetPaginated<Category, CategoryId>>;

internal sealed class GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IQueryHandler<GetCategoriesQuery, KeysetPaginated<Category, CategoryId>>
{
    public async Task<Result<KeysetPaginated<Category, CategoryId>>> Handle(
        GetCategoriesQuery query,
        CancellationToken ct = default
    )
    {
        var data = await categoryRepository.GetCategoriesAsync(
            query.Search,
            query.KeysetPagination,
            false,
            ct
        );

        return Result<KeysetPaginated<Category, CategoryId>>.Success(data);
    }
}
