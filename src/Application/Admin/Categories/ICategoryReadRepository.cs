using Application.Admin.Categories.Types;
using Domain.Shared.ValueObjects;

namespace Application.Admin.Categories;

public interface ICategoryReadRepository
{
    Task<KeysetPaginated<AdminCategoryResponse, Guid>> GetCategoriesAsync(
        NonEmptyString? search,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    );
}
