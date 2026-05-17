using Presentation.Admin.Brands.Endpoints;
using Presentation.Admin.Categories.Endpoints;
using Presentation.Admin.Products.Endpoints;
using Presentation.Admin.Products.ProductSkus.Endpoints;
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

        return await Request("/api/v1/admin/users", cookie, query, ct);
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

        return await Request("/api/v1/admin/brands", cookie, query, ct);
    }

    protected async Task<HttpResponseMessage> CreateBrand(
        CreateBrandRequest data,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/admin/brands", cookie, ct);

    protected async Task<HttpResponseMessage> UpdateBrand(
        Guid brandId,
        UpdateBrandRequest data,
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

        return await Request("/api/v1/admin/categories", cookie, query, ct);
    }

    protected async Task<HttpResponseMessage> CreateCategory(
        CreateCategoryRequest data,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/admin/categories", cookie, ct);

    protected async Task<HttpResponseMessage> UpdateCategory(
        Guid categoryId,
        UpdateCategoryRequest data,
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
    ) => await Request("/api/v1/admin/products/filters", cookie, ct: ct);

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

        return await Request("/api/v1/admin/products", cookie, query, ct);
    }

    protected async Task<HttpResponseMessage> CreateProduct(
        CreateProductRequest data,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/admin/products", cookie, ct);

    protected async Task<HttpResponseMessage> UpdateProduct(
        Guid productId,
        UpdateProductRequest data,
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

    protected async Task<HttpResponseMessage> GetProductSkus(
        GetProductSkusRequest? data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var query = new Dictionary<string, string?>
        {
            ["inStock"] = data?.InStock?.ToString(),
            ["minPrice"] = data?.MinPrice?.ToString(),
            ["maxPrice"] = data?.MaxPrice?.ToString(),
            ["minSalePrice"] = data?.MinSalePrice?.ToString(),
            ["maxSalePrice"] = data?.MaxSalePrice?.ToString(),
            ["size"] = data?.Size?.ToString(),
            ["color"] = data?.Color,
            ["sku"] = data?.Sku,
            ["limit"] = data?.Limit?.ToString(),
            ["prevCursor"] = data?.PrevCursor,
            ["nextCursor"] = data?.NextCursor,
        }
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);

        return await Request("/api/v1/admin/products/skus", cookie, query, ct);
    }

    protected async Task<HttpResponseMessage> CreateProductSku(
        Guid productId,
        CreateProductSkuRequest data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var formData = new MultipartFormDataContent
        {
            { new StringContent(data.Price.ToString()), "price" },
            { new StringContent(data.Quantity.ToString()), "quantity" },
            { new StringContent(data.Size.ToString()), "size" },
            { new StringContent(data.Color), "color" },
            { new StringContent(data.Sku), "sku" },
        };

        if (data.SalePrice is not null)
        {
            formData.Add(new StringContent(data.SalePrice.Value.ToString()), "salePrice");
        }

        foreach (var image in data.Images)
        {
            var streamContent = new StreamContent(image.OpenReadStream());

            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                image.ContentType
            );

            formData.Add(streamContent, "images", image.FileName);
        }

        return await Request(
            formData,
            HttpMethod.Post,
            $"/api/v1/admin/products/{productId}/skus",
            cookie,
            ct
        );
    }

    protected async Task<HttpResponseMessage> UpdateProductSku(
        Guid productSkuId,
        UpdateProductSkuRequest data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var formData = new MultipartFormDataContent();

        if (data.Price is not null)
            formData.Add(new StringContent(data.Price.Value.ToString()), "price");

        if (data.SalePrice is not null)
            formData.Add(new StringContent(data.SalePrice.Value.ToString()), "salePrice");

        if (data.Quantity is not null)
            formData.Add(new StringContent(data.Quantity.Value.ToString()), "quantity");

        if (data.Size is not null)
            formData.Add(new StringContent(data.Size.Value.ToString()), "size");

        if (data.Color is not null)
            formData.Add(new StringContent(data.Color), "color");

        if (data.Sku is not null)
            formData.Add(new StringContent(data.Sku), "sku");

        if (data.Images is not null)
        {
            foreach (var image in data.Images)
            {
                var streamContent = new StreamContent(image.OpenReadStream());

                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);

                formData.Add(streamContent, "images", image.FileName);
            }
        }

        return await Request(
            formData,
            HttpMethod.Patch,
            $"/api/v1/admin/products/skus/{productSkuId}",
            cookie,
            ct
        );
    }

    protected async Task<HttpResponseMessage> DeleteProductSku(
        Guid productSkuId,
        string? cookie,
        CancellationToken ct = default
    ) =>
        await Request<object>(
            null,
            HttpMethod.Delete,
            $"/api/v1/admin/products/skus/{productSkuId}",
            cookie,
            ct
        );
}
