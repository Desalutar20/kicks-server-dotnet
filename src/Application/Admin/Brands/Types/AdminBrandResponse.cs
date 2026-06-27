namespace Application.Admin.Brands.Types;

public sealed record AdminBrandResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public required string Name { get; init; }
}

internal static class AdminBrandResponseMapper
{
    public static AdminBrandResponse ToDto(this Brand brand)
    {
        return new AdminBrandResponse()
        {
            Id = brand.Id.Value,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt,
            Name = brand.Name,
        };
    }
}
