using Domain.Product.Category;
using Domain.Shared.Pagination;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Product;

internal sealed class CategoryRepository(AppDbContext dbContext)
    : RepositoryBase<Category>(dbContext),
        ICategoryRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public void CreateCategory(Category category) => Create(category);

    public void DeleteCategory(Category category) => Delete(category);

    public async Task<KeysetPaginated<Category, CategoryId>> GetCategoriesAsync(
        NonEmptyString? search,
        KeysetPagination<CategoryId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.Categories.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        var searchTerm = search?.Value.ToLower();

        query = query
            .WhereNotNull(searchTerm, u => ((string)u.Name).ToLower().StartsWith(searchTerm!))
            .ApplyKeysetPagination(keysetPagination);

        var result = await query.ToListAsync(ct);

        return result.ToKeysetPaginated(keysetPagination, u => u.CreatedAt, u => u.Id);
    }

    public async Task<Category?> GetCategoryByNameAsync(
        CategoryName name,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(x => x.Name == name, trackChanges).FirstOrDefaultAsync(ct);

    public async Task<Category?> GetCategoryByIdAsync(
        CategoryId id,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync(ct);

    public void UpdateCategory(Category category) => Update(category);
}
