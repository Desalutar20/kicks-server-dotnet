using Domain.Brand;
using Domain.Category;
using Domain.Product;
using Domain.Product.ProductSku;
using Microsoft.EntityFrameworkCore;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<User?> GetUserFromDbByEmail(Email email, CancellationToken ct) =>
        await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email, ct);

    protected async Task<User?> GetUserFromDbById(UserId id, CancellationToken ct) =>
        await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id, ct);

    protected async Task DeleteUserFromDbByEmail(Email email, CancellationToken ct) =>
        await _dbContext.Users.Where(e => e.Email == email).ExecuteDeleteAsync(ct);

    protected async Task BanUserInDbByEmail(Email email, CancellationToken ct) =>
        await _dbContext
            .Users.Where(u => u.Email == email)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.IsBanned, true), ct);

    protected async Task<Brand?> GetBrandFromDbByName(BrandName name, CancellationToken ct) =>
        await _dbContext.Brands.AsNoTracking().SingleOrDefaultAsync(b => b.Name == name, ct);

    protected async Task<Brand?> GetBrandFromDbById(BrandId id, CancellationToken ct) =>
        await _dbContext.Brands.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id, ct);

    protected async Task<Category?> GetCategoryFromDbByName(
        CategoryName name,
        CancellationToken ct
    ) => await _dbContext.Categories.AsNoTracking().SingleOrDefaultAsync(c => c.Name == name, ct);

    protected async Task<Category?> GetCategoryFromDbById(CategoryId id, CancellationToken ct) =>
        await _dbContext.Categories.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, ct);

    protected async Task<Product?> GetProductFromDbById(ProductId id, CancellationToken ct) =>
        await _dbContext.Products.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, ct);

    protected async Task<ProductSku?> GetProductSkuFromDbById(
        ProductSkuId id,
        CancellationToken ct
    ) =>
        await _dbContext
            .ProductSkus.AsNoTracking()
            .Include(x => x.ProductSkuImages)
            .SingleOrDefaultAsync(c => c.Id == id, ct);
}
