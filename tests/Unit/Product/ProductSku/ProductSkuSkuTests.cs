using System.Text.RegularExpressions;
using Domain.Products.ProductSkus;

namespace Unit.Product.ProductSku;

public class ProductSkuSkuTests
{
    private static readonly Regex Regex = new(@"\s+");

    [Theory]
    [InlineData("Sku")]
    [InlineData("   Sku multiple word   ")]
    public void Create_ValidProductSkuSku_ReturnsSuccess(string sku)
    {
        var result = ProductSkuSku.Create(sku);

        Assert.True(result.IsSuccess);
        Assert.Equal(Regex.Replace(sku, ""), result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidProductSkuSku_ReturnsFailure(string sku)
    {
        var result = ProductSkuSku.Create(sku);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_ProductSkuSkuExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', ProductSkuSku.MaxLength + 1);

        var result = ProductSkuSku.Create(value);
        Assert.True(result.IsFailure);
    }
}
