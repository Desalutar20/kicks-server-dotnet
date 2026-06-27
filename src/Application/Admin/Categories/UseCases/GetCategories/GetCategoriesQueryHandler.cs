using Application.Admin.Categories.Types;
using Domain.Shared.ValueObjects;

namespace Application.Admin.Categories.UseCases.GetCategories;

public sealed record GetCategoriesQuery(
    NonEmptyString? Search,
    KeysetPagination<Guid> KeysetPagination
) : IQuery<KeysetPaginated<AdminCategoryResponse, Guid>>;

internal sealed class GetCategoriesQueryHandler(ICategoryReadRepository categoryReadRepository)
    : IQueryHandler<GetCategoriesQuery, KeysetPaginated<AdminCategoryResponse, Guid>>
{
    public async Task<Result<KeysetPaginated<AdminCategoryResponse, Guid>>> Handle(
        GetCategoriesQuery query,
        CancellationToken ct = default
    )
    {
        var data = await categoryReadRepository.GetCategoriesAsync(
            query.Search,
            query.KeysetPagination,
            ct
        );

        return data;
    }
}
