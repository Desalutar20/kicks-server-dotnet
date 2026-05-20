using Domain.Product;

namespace Presentation.ProductSkus.Dto;

public sealed record ProductDto(
    string Title,
    string Description,
    ProductGender Gender,
    List<string> Tags,
    Guid? BrandId,
    Guid? CategoryId
);

internal static class ProductDtoMapper
{
    public static ProductDto ToDto(this Product model) =>
        new(
            model.Title.Value,
            model.Description.Value,
            model.Gender,
            model.Tags.Value,
            model.BrandId?.Value,
            model.CategoryId?.Value
        );
}
