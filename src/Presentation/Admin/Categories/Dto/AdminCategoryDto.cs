using Domain.Category;

namespace Presentation.Admin.Categories.Dto;

public sealed record AdminCategoryDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Name
);

internal static class AdminCategoryDtoMapper
{
    public static AdminCategoryDto ToDto(this Category category) =>
        new(category.Id.Value, category.CreatedAt, category.UpdatedAt, category.Name.ToString());
}
