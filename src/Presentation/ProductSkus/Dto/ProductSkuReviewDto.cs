using Domain.Products.ProductSkus.ProductSkuReviews;

namespace Presentation.ProductSkus.Dto;

public sealed record ProductSkuReviewDto(string Description, int Rating, List<FileDto> Images);

internal static class ProductSkuReviewDtoMapper
{
    public static ProductSkuReviewDto ToDto(this ProductSkuReview model) =>
        new(
            model.Description.Value,
            model.Rating.Value,
            [.. model.Images.Select(image => image.ToDto())]
        );
}
