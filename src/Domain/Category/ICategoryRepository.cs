using Domain.Shared;
using Domain.Shared.Pagination;

namespace Domain.Category;

public interface ICategoryRepository
{
    Task<KeysetPaginated<Category, CategoryId>> GetCategoriesAsync(
        NonEmptyString? search,
        KeysetPagination<CategoryId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<Category?> GetCategoryByNameAsync(
        CategoryName name,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<Category?> GetCategoryByIdAsync(
        CategoryId id,
        bool trackChanges,
        CancellationToken ct = default
    );

    void CreateCategory(Category category);
    void UpdateCategory(Category category);
    void DeleteCategory(Category category);
}
