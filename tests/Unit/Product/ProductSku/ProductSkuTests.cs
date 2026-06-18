using Domain.Products;
using Domain.Products.ProductSkus;
using Domain.Shared.FileContent;
using Domain.Shared.ValueObjects;
using FluentAssertions;

namespace Unit.Product.ProductSku;

public class ProductSkuTests
{
    [Fact]
    public void Create_Should_Create_ProductSku()
    {
        var price = ProductSkuPrice
            .Create(Money.FromCents(100).Value, Money.FromCents(80).Value)
            .Value;

        var quantity = PositiveInt.Create(10).Value;
        var size = PositiveInt.Create(42).Value;
        var color = ProductSkuColor.Create("#000000").Value;
        var sku = ProductSkuSku.Create("NK-001").Value;

        var images = new List<ProductSkuImage> { CreateImage(1) };

        var result = Domain.Products.ProductSkus.ProductSku.Create(
            price,
            quantity,
            color,
            sku,
            size,
            images,
            new ProductId(Guid.NewGuid())
        );

        result.IsSuccess.Should().BeTrue();

        var productSku = result.Value;

        productSku.Price.Should().Be(price);
        productSku.Quantity.Should().Be(quantity);
        productSku.Size.Should().Be(size);
        productSku.Color.Should().Be(color);
        productSku.Sku.Should().Be(sku);
        productSku.Images.Should().HaveCount(1);
    }

    [Fact]
    public void AddImages_Should_Add_Images()
    {
        var sku = CreateValidProductSku();

        var newImages = new List<ProductSkuImage> { CreateImage(2) };

        var result = sku.AddImages(newImages);

        result.IsSuccess.Should().BeTrue();
        sku.Images.Should().HaveCount(2);
    }

    [Fact]
    public void AddImages_Should_Return_Error_When_MaxImages_Exceeded()
    {
        var sku = CreateValidProductSku(Domain.Products.ProductSkus.ProductSku.MaxImages);

        var newImages = new List<ProductSkuImage> { CreateImage(5) };

        var result = sku.AddImages(newImages);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RemoveImage_Should_Remove_Image()
    {
        var sku = CreateValidProductSku(2);

        var imageId = sku.Images[0].Id;

        var result = sku.RemoveImage(imageId);

        result.IsSuccess.Should().BeTrue();
        sku.Images.Should().HaveCount(1);
        sku.Images.Should().NotContain(x => x.Id == imageId);
    }

    [Fact]
    public void RemoveImage_Should_Return_Error_When_Last_Image()
    {
        var sku = CreateValidProductSku();

        var imageId = sku.Images[0].Id;

        var result = sku.RemoveImage(imageId);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Update_Should_Update_Fields()
    {
        var sku = CreateValidProductSku();

        var newPrice = ProductSkuPrice
            .Create(Money.FromCents(2000).Value, Money.FromCents(1500).Value)
            .Value;

        sku.Update(
            newPrice,
            PositiveInt.Create(20).Value,
            PositiveInt.Create(44).Value,
            ProductSkuColor.Create("#000000").Value,
            ProductSkuSku.Create("NEW-SKU").Value
        );

        sku.Price.Price.Dollars.Should().Be(20);
        sku.Price.SalePrice!.Dollars.Should().Be(15);
        sku.Quantity.Should().Be(20);
        sku.Size.Value.Should().Be(44);
        sku.Color.Value.Should().Be("#000000");
        sku.Sku.Value.Should().Be("NEW-SKU");
    }

    private static Domain.Products.ProductSkus.ProductSku CreateValidProductSku(int imageCount = 1)
    {
        var images = Enumerable.Range(1, imageCount).Select(CreateImage).ToList();

        var productSku = Domain
            .Products.ProductSkus.ProductSku.Create(
                ProductSkuPrice.Create(Money.FromCents(100).Value, Money.FromCents(80).Value).Value,
                PositiveInt.Create(10).Value,
                ProductSkuColor.Create("#ffffff").Value,
                ProductSkuSku.Create("SKU-1").Value,
                PositiveInt.Create(42).Value,
                images,
                new ProductId(Guid.NewGuid())
            )
            .Value;

        return productSku;
    }

    private static ProductSkuImage CreateImage(int index)
    {
        return ProductSkuImage.Create(
            Guid.NewGuid(),
            FileUrl.Create($"https://cdn.test.com/image-{index}.webp").Value,
            FileName.Create($"image-{index}.webp").Value
        );
    }
}
