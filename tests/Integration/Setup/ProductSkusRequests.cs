using Presentation.ProductSkus.Endpoints;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<HttpResponseMessage> GetProductSkus(
        GetProductSkusRequest? data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var query = QueryBuilder.Create();

        query.AddRange("sizes", data?.Sizes);
        query.AddRange("colors", data?.Colors);
        query.AddRange("categoryIds", data?.CategoryIds);
        query.AddRange("brandIds", data?.BrandIds);
        query.AddRange("genders", data?.Genders);

        query.Add("minPrice", data?.MinPrice?.ToString());
        query.Add("maxPrice", data?.MaxPrice?.ToString());
        query.Add("limit", data?.Limit?.ToString());
        query.Add("prevCursor", data?.PrevCursor);
        query.Add("nextCursor", data?.NextCursor);

        return await Request("/api/v1/product-skus", cookie, query, ct);
    }
}
