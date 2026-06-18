using Presentation.Orders.Endpoints;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<HttpResponseMessage> CreateOrder(
        CreateOrderRequest data,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/orders", cookie, ct);

    protected async Task<HttpResponseMessage> GetOrders(
        GetOrdersRequest? data,
        string? cookie,
        CancellationToken ct = default
    )
    {
        var query = new Dictionary<string, string?>
        {
            ["limit"] = data?.Limit?.ToString(),
            ["prevCursor"] = data?.PrevCursor,
            ["nextCursor"] = data?.NextCursor,
        }
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);

        return await Request("/api/v1/orders", cookie, query, ct);
    }

    protected async Task<HttpResponseMessage> GetOrder(
        Guid orderId,
        string? cookie,
        CancellationToken ct = default
    ) => await Request($"/api/v1/orders/{orderId}", cookie, null, ct);
}
