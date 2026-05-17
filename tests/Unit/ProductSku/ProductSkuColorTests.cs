using Domain.Product.ProductSku;

namespace Unit.ProductSku;

public class ProductSkuColorTests
{
    [Theory]
    [InlineData("#ff00ff")]
    [InlineData("   #000000   ")]
    public void Create_ValidProductSkuColor_ReturnsSuccess(string color)
    {
        var result = ProductSkuColor.Create(color);

        Assert.True(result.IsSuccess);
        Assert.Equal(color.Trim(), result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Not hex")]
    [InlineData("#gggggg")]
    [InlineData("ff00ff")]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    public void Create_InvalidProductSkuColor_ReturnsFailure(string color)
    {
        var result = ProductSkuColor.Create(color);

        Assert.True(result.IsFailure);
    }
}
