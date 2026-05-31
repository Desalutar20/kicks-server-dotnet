using Domain.Products.ProductSkus;
using Domain.Shared;
using FluentAssertions;

namespace Unit.Product.ProductSku;

public class ProductSkuPriceTests
{
    [Fact]
    public void Create_ValidPriceWithoutSalePrice_ReturnsSuccess()
    {
        var price = PositiveInt.Create(100).Value;

        var result = ProductSkuPrice.Create(price, null);

        result.IsSuccess.Should().BeTrue();

        result.Value.Price.Value.Should().Be(100);
        result.Value.SalePrice.Should().BeNull();
        result.Value.CurrentPrice.Value.Should().Be(100);
    }

    [Fact]
    public void Create_ValidPriceWithValidSalePrice_ReturnsSuccess()
    {
        var price = PositiveInt.Create(100).Value;
        var salePrice = PositiveInt.Create(80).Value;

        var result = ProductSkuPrice.Create(price, salePrice);

        result.IsSuccess.Should().BeTrue();

        result.Value.Price.Value.Should().Be(100);
        result.Value.SalePrice!.Value.Should().Be(80);
        result.Value.CurrentPrice.Value.Should().Be(80);
    }

    [Fact]
    public void Create_SalePriceEqualToPrice_ReturnsFailure()
    {
        var price = PositiveInt.Create(100).Value;
        var salePrice = PositiveInt.Create(100).Value;

        var result = ProductSkuPrice.Create(price, salePrice);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_SalePriceGreaterThanPrice_ReturnsFailure()
    {
        var price = PositiveInt.Create(100).Value;
        var salePrice = PositiveInt.Create(150).Value;

        var result = ProductSkuPrice.Create(price, salePrice);

        result.IsFailure.Should().BeTrue();
    }
}
