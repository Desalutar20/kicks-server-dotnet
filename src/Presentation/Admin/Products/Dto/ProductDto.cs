using Domain.Product;

namespace Presentation.Admin.Products.Dto;

public sealed record ProductDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Title,
    string Description,
    ProductGender Gender,
    List<string> Tags,
    Guid? BrandId,
    Guid? CategoryId,
    bool IsDeleted
);

internal static class ProductDtoMapper
{
    public static ProductDto ToDto(this Product model) =>
        new(
            model.Id.Value,
            model.CreatedAt,
            model.UpdatedAt,
            model.Title.Value,
            model.Description.Value,
            model.Gender,
            model.Tags.Value,
            model.BrandId?.Value,
            model.CategoryId?.Value,
            model.IsDeleted
        );
}
