using Presentation.ProductSkus.Dto;
using Presentation.Shared.Dto;

namespace Integration.ProductSkus;

public sealed class GetProductSkuTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetProductSkus(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuDto>>(ct);
        body.Should().NotBeNull();

        var getProductSkuResponse = await GetProductSku(body.Data[0].Id, ct);
        getProductSkuResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getProductSkuBody = await getProductSkuResponse.Content.ReadFromJsonAsync<
            ApiResponse<ProductSkuDto>
        >(ct);
        getProductSkuBody.Should().NotBeNull();
    }
}
