using Domain.Products.ProductSkus;
using Domain.Shared.ValueObjects;
using FluentAssertions;

namespace Unit.Product.ProductSku;

public class ProductSkuPriceTests
{
    [Fact]
    public void Create_ValidPriceWithoutSalePrice_ReturnsSuccess()
    {
        var price = Money.FromCents(100).Value;

        var result = ProductSkuPrice.Create(price, null);

        result.IsSuccess.Should().BeTrue();

        result.Value.Price.Cents.Should().Be(100);
        result.Value.SalePrice.Should().BeNull();
        result.Value.CurrentPrice.Cents.Should().Be(100);
    }

    [Fact]
    public void Create_ValidPriceWithValidSalePrice_ReturnsSuccess()
    {
        var price = Money.FromCents(100).Value;
        var salePrice = Money.FromCents(80).Value;

        var result = ProductSkuPrice.Create(price, salePrice);

        result.IsSuccess.Should().BeTrue();

        result.Value.Price.Cents.Should().Be(100);
        result.Value.SalePrice!.Cents.Should().Be(80);
        result.Value.CurrentPrice.Cents.Should().Be(80);
    }

    [Fact]
    public void Create_SalePriceEqualToPrice_ReturnsFailure()
    {
        var price = Money.FromCents(100).Value;
        var salePrice = Money.FromCents(100).Value;

        var result = ProductSkuPrice.Create(price, salePrice);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_SalePriceGreaterThanPrice_ReturnsFailure()
    {
        var price = Money.FromCents(100).Value;
        var salePrice = Money.FromCents(150).Value;

        var result = ProductSkuPrice.Create(price, salePrice);

        result.IsFailure.Should().BeTrue();
    }
}
