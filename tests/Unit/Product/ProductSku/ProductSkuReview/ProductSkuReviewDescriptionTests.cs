using Domain.Products.ProductSkus.ProductSkuReviews;

namespace Unit.Product.ProductSku.ProductSkuReview;

public class ProductSkuReviewDescriptionTests
{
    [Theory]
    [InlineData("Very comfortable sneakers, perfect for everyday walking.")]
    [InlineData("The quality is amazing and the size fits exactly as expected.")]
    [InlineData("Lightweight shoes with great cushioning for running.")]
    [InlineData("Stylish design and comfortable even after long hours of wear.")]
    [InlineData("Good materials, fast delivery, and overall satisfied with the purchase.")]
    [InlineData("   Excellent sneakers, comfortable sole and premium feel.   ")]
    public void Create_ValidProductSkuReview_ReturnsSuccess(string description)
    {
        var result = ProductSkuReviewDescription.Create(description);

        Assert.True(result.IsSuccess);
        Assert.Equal(description.Trim(), result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidProductSkuReview_ReturnsFailure(string description)
    {
        var result = ProductSkuReviewDescription.Create(description);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_ProductSkuReviewExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', ProductSkuReviewDescription.MaxLength + 1);

        var result = ProductSkuReviewDescription.Create(value);
        Assert.True(result.IsFailure);
    }
}
