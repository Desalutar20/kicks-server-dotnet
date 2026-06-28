namespace Application.Admin.Categories.Types;

public sealed record AdminCategoryResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public required string Name { get; init; }
}

internal static class AdminCategoryResponseMapper
{
    public static AdminCategoryResponse ToResponse(this Category category)
    {
        return new AdminCategoryResponse()
        {
            Id = category.Id.Value,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            Name = category.Name,
        };
    }
}
