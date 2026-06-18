namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<HttpResponseMessage> GetDeliveryOptions(
        string? cookie,
        CancellationToken ct = default
    ) => await Request("/api/v1/delivery-options", cookie, null, ct);
}
