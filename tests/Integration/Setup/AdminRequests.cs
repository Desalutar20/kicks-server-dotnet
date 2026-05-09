using Application.Admin.Products.UseCases.GetProducts;
using Presentation.Admin.Brands.Endpoints;
using Presentation.Admin.Categories.Endpoints;
using Presentation.Admin.Products.Endpoints;
using Presentation.Admin.Users.Endpoints;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<HttpResponseMessage> GetAdminUsers(
        GetAdminUsersRequest? data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var query = new Dictionary<string, string?>
        {
            ["search"] = data?.Search,
            ["isBanned"] = data?.IsBanned?.ToString(),
            ["isVerified"] = data?.IsVerified?.ToString(),
            ["gender"] = data?.Gender,
            ["limit"] = data?.Limit?.ToString(),
            ["prevCursor"] = data?.PrevCursor,
            ["nextCursor"] = data?.NextCursor,
        }
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);

        return await Request("/api/v1/admin/users", cookie, ct, query);
    }

    protected async Task<HttpResponseMessage> ToggleBanUser(
        Guid userId,
        string? cookie,
        CancellationToken ct = default
    ) => await Request<object>(null, HttpMethod.Post, $"/api/v1/admin/users/{userId}", cookie, ct);

    protected async Task<HttpResponseMessage> DeleteUser(
        Guid userId,
        string? cookie,
        CancellationToken ct = default
    ) =>
        await Request<object>(null, HttpMethod.Delete, $"/api/v1/admin/users/{userId}", cookie, ct);

    protected async Task<HttpResponseMessage> GetBrands(
        GetBrandsRequest? data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var query = new Dictionary<string, string?>
        {
            ["search"] = data?.Search,
            ["limit"] = data?.Limit?.ToString(),
            ["prevCursor"] = data?.PrevCursor,
            ["nextCursor"] = data?.NextCursor,
        }
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);

        return await Request("/api/v1/admin/brands", cookie, ct, query);
    }

    protected async Task<HttpResponseMessage> CreateBrand(
        CreateBrandRequest data,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/admin/brands", cookie, ct);

    protected async Task<HttpResponseMessage> UpdateBrand(
        UpdateBrandRequest data,
        Guid brandId,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Patch, $"/api/v1/admin/brands/{brandId}", cookie, ct);

    protected async Task<HttpResponseMessage> DeleteBrand(
        Guid brandId,
        string? cookie,
        CancellationToken ct = default
    ) =>
        await Request<object>(
            null,
            HttpMethod.Delete,
            $"/api/v1/admin/brands/{brandId}",
            cookie,
            ct
        );

    protected async Task<HttpResponseMessage> GetCategories(
        GetCategoriesRequest? data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var query = new Dictionary<string, string?>
        {
            ["search"] = data?.Search,
            ["limit"] = data?.Limit?.ToString(),
            ["prevCursor"] = data?.PrevCursor,
            ["nextCursor"] = data?.NextCursor,
        }
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);

        return await Request("/api/v1/admin/categories", cookie, ct, query);
    }

    protected async Task<HttpResponseMessage> CreateCategory(
        CreateCategoryRequest data,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/admin/categories", cookie, ct);

    protected async Task<HttpResponseMessage> UpdateCategory(
        UpdateCategoryRequest data,
        Guid categoryId,
        string? cookie,
        CancellationToken ct = default
    ) =>
        await Request(data, HttpMethod.Patch, $"/api/v1/admin/categories/{categoryId}", cookie, ct);

    protected async Task<HttpResponseMessage> DeleteCategory(
        Guid categoryId,
        string? cookie,
        CancellationToken ct = default
    ) =>
        await Request<object>(
            null,
            HttpMethod.Delete,
            $"/api/v1/admin/categories/{categoryId}",
            cookie,
            ct
        );

    protected async Task<HttpResponseMessage> GetProductFilters(
        string? cookie,
        CancellationToken ct = default
    ) => await Request("/api/v1/admin/products/filters", cookie, ct);

    protected async Task<HttpResponseMessage> GetProducts(
        GetProductsRequest? data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var query = new Dictionary<string, string?>
        {
            ["search"] = data?.Search,
            ["gender"] = data?.Gender,
            ["brandId"] = data?.BrandId,
            ["categoryId"] = data?.CategoryId,
            ["isDeleted"] = data?.IsDeleted?.ToString(),
            ["limit"] = data?.Limit?.ToString(),
            ["prevCursor"] = data?.PrevCursor,
            ["nextCursor"] = data?.NextCursor,
        }
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);

        return await Request("/api/v1/admin/products", cookie, ct, query);
    }

    protected async Task<HttpResponseMessage> CreateProduct(
        CreateProductRequest data,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/admin/products", cookie, ct);

    protected async Task<HttpResponseMessage> UpdateProduct(
        UpdateProductRequest data,
        Guid productId,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Patch, $"/api/v1/admin/products/{productId}", cookie, ct);

    protected async Task<HttpResponseMessage> ToggleProductIsDeleted(
        Guid productId,
        string? cookie,
        CancellationToken ct = default
    ) =>
        await Request<object?>(
            null,
            HttpMethod.Post,
            $"/api/v1/admin/products/{productId}",
            cookie,
            ct
        );
}
