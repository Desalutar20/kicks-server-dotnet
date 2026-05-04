using Domain.Product.Category;

namespace Presentation.Admin.Categories.Dto;

public sealed record CategoryDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Name
);

internal static class CategoryDtoMapper
{
    public static CategoryDto ToDto(this Category category) =>
        new(category.Id.Value, category.CreatedAt, category.UpdatedAt, category.Name.ToString());
}
