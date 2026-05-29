using Domain.Products.ProductSkus.ProductSkuReviews;

namespace Unit.Product.ProductSku.ProductSkuReview;

public class ProductSkuReviewRatingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Create_ValidRating_ReturnsSuccess(int rating)
    {
        var result = ProductSkuReviewRating.Create(rating);

        Assert.True(result.IsSuccess);
        Assert.Equal(rating, result.Value.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public void Create_InvalidRating_ReturnsFailure(int rating)
    {
        var result = ProductSkuReviewRating.Create(rating);

        Assert.True(result.IsFailure);
    }
}
