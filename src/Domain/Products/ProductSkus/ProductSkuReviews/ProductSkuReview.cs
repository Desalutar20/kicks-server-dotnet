using Domain.Abstractions;
using Domain.Users;

namespace Domain.Products.ProductSkus.ProductSkuReviews;

public sealed class ProductSkuReview : Entity<ProductSkuReviewId>
{
    public const int MaxImages = 4;

    private ProductSkuReview(
        ProductSkuReviewDescription description,
        ProductSkuReviewRating rating,
        ProductSkuId productSkuId,
        UserId userId
    )
        : base(new ProductSkuReviewId(Guid.NewGuid()))
    {
        Description = description;
        Rating = rating;
        ProductSkuId = productSkuId;
        UserId = userId;
    }

    public ProductSkuReviewDescription Description { get; private set; }
    public ProductSkuReviewRating Rating { get; private set; }
    public ProductSkuReviewStatus Status { get; private set; } = ProductSkuReviewStatus.Pending;

    private readonly List<ProductSkuReviewImage> _images = [];
    public IReadOnlyList<ProductSkuReviewImage> Images => _images;

    public ProductSkuId ProductSkuId { get; private set; }
    public UserId UserId { get; private set; }

    public static Result<ProductSkuReview> Create(
        ProductSkuReviewDescription description,
        ProductSkuReviewRating rating,
        ProductSkuId productSkuId,
        UserId userId,
        List<ProductSkuReviewImage> images
    )
    {
        if (images.Count > MaxImages)
        {
            return Error.Validation(
                "productSkuReviewImages",
                [$"Product SKU review must contain a maximum of {MaxImages} images."]
            );
        }

        var productSkuReview = new ProductSkuReview(description, rating, productSkuId, userId);

        productSkuReview._images.AddRange(images);

        return productSkuReview;
    }
}
