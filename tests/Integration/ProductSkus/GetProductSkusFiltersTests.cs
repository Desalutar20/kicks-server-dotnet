using Application.ProductSkus.Types;
using Presentation.Shared.Dto;

namespace Integration.ProductSkus;

public sealed class GetProductSkusFiltersTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetProductSkusFilters(ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductSkusFilterOptions>>(
            ct
        );
        body.Should().NotBeNull();
    }
}
