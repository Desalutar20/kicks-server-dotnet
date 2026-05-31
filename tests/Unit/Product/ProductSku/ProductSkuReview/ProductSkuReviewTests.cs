using Domain.Products.ProductSkus;
using Domain.Products.ProductSkus.ProductSkuReviews;
using Domain.Shared.FileContent;
using Domain.Users;
using FluentAssertions;

namespace Unit.Product.ProductSku.ProductSkuReview;

public class ProductSkuReviewTests
{
    [Fact]
    public void Create_Should_Create_ProductSkuReview_When_Data_Is_Valid()
    {
        var description = ProductSkuReviewDescription
            .Create("Very comfortable sneakers with great build quality.")
            .Value;

        var rating = ProductSkuReviewRating.Create(5).Value;

        var images = new List<ProductSkuReviewImage>();

        var result = Domain.Products.ProductSkus.ProductSkuReviews.ProductSkuReview.Create(
            description,
            rating,
            new ProductSkuId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            images
        );

        result.IsSuccess.Should().BeTrue();

        var review = result.Value;

        review.Description.Should().Be(description);
        review.Rating.Should().Be(rating);
        review.Status.Should().Be(ProductSkuReviewStatus.Pending);
        review.Images.Should().BeEmpty();
    }

    [Fact]
    public void Create_Should_Fail_When_Too_Many_Images()
    {
        var description = ProductSkuReviewDescription.Create("Good product").Value;
        var rating = ProductSkuReviewRating.Create(4).Value;

        var images = Enumerable
            .Range(1, Domain.Products.ProductSkus.ProductSkuReviews.ProductSkuReview.MaxImages + 1)
            .Select(_ =>
                ProductSkuReviewImage.Create(
                    Guid.NewGuid(),
                    FileUrl.Create("https://cdn.test.com/image.webp").Value,
                    FileName.Create("image.webp").Value
                )
            )
            .ToList();

        var result = Domain.Products.ProductSkus.ProductSkuReviews.ProductSkuReview.Create(
            description,
            rating,
            new ProductSkuId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            images
        );

        result.IsFailure.Should().BeTrue();
    }
}
