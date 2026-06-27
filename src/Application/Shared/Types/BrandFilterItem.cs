namespace Application.Shared.Types;

public sealed record BrandFilterItem
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
};
