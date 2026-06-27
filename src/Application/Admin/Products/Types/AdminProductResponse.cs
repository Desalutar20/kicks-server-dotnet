namespace Application.Admin.Products.Types;

public sealed record AdminProductResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public ProductGender Gender { get; init; }
    public List<string> Tags { get; init; } = [];
    public Guid? BrandId { get; init; }
    public Guid? CategoryId { get; init; }
    public bool IsDeleted { get; init; }
}

internal static class AdminProductResponseMapper
{
    public static AdminProductResponse ToDto(this Product product)
    {
        return new AdminProductResponse()
        {
            Id = product.Id.Value,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Title = product.Title.Value,
            Description = product.Description.Value,
            Gender = product.Gender,
            Tags = product.Tags.Value,
            BrandId = product.BrandId?.Value,
            CategoryId = product.CategoryId?.Value,
            IsDeleted = product.IsDeleted,
        };
    }
}
