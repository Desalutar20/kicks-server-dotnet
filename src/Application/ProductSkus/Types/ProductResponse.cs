namespace Application.ProductSkus.Types;

public sealed record ProductResponse
{
    public Guid ProductId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public ProductGender Gender { get; init; }
    public List<string> Tags { get; init; } = [];
    public Guid? BrandId { get; init; }
    public Guid? CategoryId { get; init; }
}
