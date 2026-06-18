using Domain.Brands;
using Domain.Categories;
using Domain.DeliveryOptions;
using Domain.Orders;
using Domain.Products;
using Domain.Products.ProductSkus;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;
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
    ) => await _dbContext.ProductSkus.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, ct);

    protected async Task<Promocode?> GetPromocodeFromDbById(PromocodeId id, CancellationToken ct) =>
        await _dbContext.Promocodes.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, ct);

    protected async Task<Domain.Carts.Cart?> GetCartByUserEmailFromDb(
        Email email,
        CancellationToken ct
    )
    {
        var user = await GetUserFromDbByEmail(email, ct);
        user.Should().NotBeNull();

        var cart = await _dbContext
            .Carts.AsNoTracking()
            .SingleOrDefaultAsync(c => c.UserId == user.Id, ct);
        return cart;
    }

    protected async Task<List<ProductSkuId>> GetProductSkuIdsFromDb(int count, CancellationToken ct)
    {
        return await _dbContext
            .ProductSkus.AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(count)
            .Select(x => x.Id)
            .ToListAsync(ct);
    }

    protected async Task<Promocode> GetPromocodeFromDb(CancellationToken ct) =>
        await _dbContext.Promocodes.AsNoTracking().Take(1).FirstAsync(ct);

    protected async Task<DeliveryOption?> GetDeliveryOptionFromDbByTitle(
        DeliveryOptionTitle title,
        CancellationToken ct
    ) =>
        await _dbContext
            .DeliveryOptions.AsNoTracking()
            .SingleOrDefaultAsync(b => b.Title == title, ct);

    protected async Task<DeliveryOption?> GetDeliveryOptionFromDbById(
        DeliveryOptionId id,
        CancellationToken ct
    ) => await _dbContext.DeliveryOptions.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id, ct);

    protected async Task<Order?> GetOrderFromDbById(OrderId id, CancellationToken ct) =>
        await _dbContext.Orders.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id, ct);

    protected async Task CancelOrderInDbById(OrderId id, CancellationToken ct) =>
        await _dbContext
            .Orders.Where(u => u.Id == id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(u => u.Status, OrderStatus.Cancelled),
                ct
            );
}
