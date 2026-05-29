using Domain.Products;
using Domain.Products.ProductSkus;
using Domain.Shared;
using Domain.Shared.FileContent;

namespace Unit.Product.ProductSku;

public class ProductSkuTests
{
    [Fact]
    public void Create_Should_Create_ProductSku()
    {
        var price = ProductSkuPrice
            .Create(PositiveInt.Create(100).Value, PositiveInt.Create(80).Value)
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
            new ProductId(Guid.NewGuid()),
            images
        );

        Assert.True(result.IsSuccess);

        var productSku = result.Value;

        Assert.Equal(price, productSku.Price);
        Assert.Equal(quantity, productSku.Quantity);
        Assert.Equal(size, productSku.Size);
        Assert.Equal(color, productSku.Color);
        Assert.Equal(sku, productSku.Sku);
        Assert.Single(productSku.Images);
    }

    [Fact]
    public void Create_Should_Return_Error_When_Images_Are_Empty()
    {
        var price = ProductSkuPrice.Create(PositiveInt.Create(100).Value, null).Value;

        var result = Domain.Products.ProductSkus.ProductSku.Create(
            price,
            PositiveInt.Create(10).Value,
            ProductSkuColor.Create("#000000").Value,
            ProductSkuSku.Create("SKU-1").Value,
            PositiveInt.Create(42).Value,
            new ProductId(Guid.NewGuid()),
            []
        );

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void AddImages_Should_Add_Images()
    {
        var sku = CreateValidProductSku();

        var newImages = new List<ProductSkuImage> { CreateImage(2) };

        var result = sku.AddImages(newImages);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, sku.Images.Count);
    }

    [Fact]
    public void AddImages_Should_Return_Error_When_MaxImages_Exceeded()
    {
        var sku = CreateValidProductSku(Domain.Products.ProductSkus.ProductSku.MaxImages);

        var newImages = new List<ProductSkuImage> { CreateImage(5) };

        var result = sku.AddImages(newImages);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void RemoveImage_Should_Remove_Image()
    {
        var sku = CreateValidProductSku(2);

        var imageId = sku.Images[0].Id;

        var result = sku.RemoveImage(imageId);

        Assert.True(result.IsSuccess);
        Assert.Single(sku.Images);
        Assert.DoesNotContain(sku.Images, x => x.Id == imageId);
    }

    [Fact]
    public void RemoveImage_Should_Return_Error_When_Last_Image()
    {
        var sku = CreateValidProductSku();

        var imageId = sku.Images[0].Id;

        var result = sku.RemoveImage(imageId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Update_Should_Update_Fields()
    {
        var sku = CreateValidProductSku();

        var newPrice = ProductSkuPrice
            .Create(PositiveInt.Create(200).Value, PositiveInt.Create(150).Value)
            .Value;

        var result = sku.Update(
            newPrice,
            PositiveInt.Create(20).Value,
            PositiveInt.Create(44).Value,
            ProductSkuColor.Create("#000000").Value,
            ProductSkuSku.Create("NEW-SKU").Value
        );

        Assert.True(result.IsSuccess);

        Assert.Equal(200, sku.Price.Price.Value);
        Assert.Equal(150, sku.Price.SalePrice!.Value);
        Assert.Equal(20, sku.Quantity.Value);
        Assert.Equal(44, sku.Size.Value);
        Assert.Equal("#000000", sku.Color.Value);
        Assert.Equal("NEW-SKU", sku.Sku.Value);
    }

    [Fact]
    public void Update_Should_Return_Error_When_SalePrice_Exceeds_Price()
    {
        var sku = CreateValidProductSku();

        var invalidPrice = ProductSkuPrice.Create(PositiveInt.Create(50).Value, null).Value;

        sku.Update(invalidPrice, null, null, null, null);

        var result = sku.Update(
            ProductSkuPrice.Create(PositiveInt.Create(70).Value, null).Value,
            null,
            null,
            null,
            null
        );

        Assert.True(result.IsFailure);
    }

    private static Domain.Products.ProductSkus.ProductSku CreateValidProductSku(int imageCount = 1)
    {
        var images = Enumerable.Range(1, imageCount).Select(CreateImage).ToList();

        return Domain
            .Products.ProductSkus.ProductSku.Create(
                ProductSkuPrice
                    .Create(PositiveInt.Create(100).Value, PositiveInt.Create(80).Value)
                    .Value,
                PositiveInt.Create(10).Value,
                ProductSkuColor.Create("#ffffff").Value,
                ProductSkuSku.Create("SKU-1").Value,
                PositiveInt.Create(42).Value,
                new ProductId(Guid.NewGuid()),
                images
            )
            .Value;
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
