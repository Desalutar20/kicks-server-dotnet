using Domain.Product.ProductSku;
using Domain.Shared;

namespace Unit.ProductSku;

public class ProductSkuPriceTests
{
    [Fact]
    public void Create_ValidPriceWithoutSalePrice_ReturnsSuccess()
    {
        var price = PositiveInt.Create(100).Value;

        var result = ProductSkuPrice.Create(price, null);

        Assert.True(result.IsSuccess);

        Assert.Equal(100, result.Value.Price.Value);
        Assert.Null(result.Value.SalePrice);
        Assert.Equal(100, result.Value.CurrentPrice.Value);
    }

    [Fact]
    public void Create_ValidPriceWithValidSalePrice_ReturnsSuccess()
    {
        var price = PositiveInt.Create(100).Value;
        var salePrice = PositiveInt.Create(80).Value;

        var result = ProductSkuPrice.Create(price, salePrice);

        Assert.True(result.IsSuccess);

        Assert.Equal(100, result.Value.Price.Value);
        Assert.Equal(80, result.Value.SalePrice!.Value);
        Assert.Equal(80, result.Value.CurrentPrice.Value);
    }

    [Fact]
    public void Create_SalePriceEqualToPrice_ReturnsFailure()
    {
        var price = PositiveInt.Create(100).Value;
        var salePrice = PositiveInt.Create(100).Value;

        var result = ProductSkuPrice.Create(price, salePrice);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_SalePriceGreaterThanPrice_ReturnsFailure()
    {
        var price = PositiveInt.Create(100).Value;
        var salePrice = PositiveInt.Create(150).Value;

        var result = ProductSkuPrice.Create(price, salePrice);

        Assert.True(result.IsFailure);
    }
}
