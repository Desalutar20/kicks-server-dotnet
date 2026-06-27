using Presentation.Cart.Endpoints;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<HttpResponseMessage> GetCart(
        string? cookie,
        CancellationToken ct = default
    ) => await Request("/api/v1/cart", cookie, null, ct);

    protected async Task<HttpResponseMessage> AddCartItem(
        Guid productSkuId,
        string? cookie,
        CancellationToken ct = default
    ) =>
        await Request<object>(
            null,
            HttpMethod.Post,
            $"/api/v1/cart/items{productSkuId}",
            cookie,
            ct
        );

    protected async Task<HttpResponseMessage> UpdateCartItemQuantity(
        Guid productSkuId,
        UpdateCartItemQuantityRequest data,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Patch, $"/api/v1/cart/items/{productSkuId}", cookie, ct);

    protected async Task<HttpResponseMessage> RemoveCartItem(
        Guid productSkuId,
        string? cookie,
        CancellationToken ct = default
    ) =>
        await Request<object>(
            null,
            HttpMethod.Delete,
            $"/api/v1/cart/items/{productSkuId}",
            cookie,
            ct
        );

    protected async Task<HttpResponseMessage> ClearCart(
        string? cookie,
        CancellationToken ct = default
    ) => await Request<object>(null, HttpMethod.Delete, "/api/v1/cart/items", cookie, ct);

    protected async Task<HttpResponseMessage> ApplyPromocode(
        ApplyPromocodeRequest data,
        string? cookie,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/cart/promocode", cookie, ct);

    protected async Task<HttpResponseMessage> RemovePromocode(
        string? cookie,
        CancellationToken ct = default
    ) => await Request<object>(null, HttpMethod.Delete, "/api/v1/cart/promocode", cookie, ct);
}
