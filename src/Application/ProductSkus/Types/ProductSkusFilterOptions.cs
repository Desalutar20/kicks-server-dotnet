using Application.Shared.Types;

namespace Application.ProductSkus.Types;

public sealed record ProductSkusFilterOptions
{
    public IReadOnlyList<int> Sizes { get; init; } = [];
    public IReadOnlyList<string> Colors { get; init; } = [];
    public IReadOnlyList<string> Genders { get; init; } = [];
    public IReadOnlyList<CategoryFilterItem> Categories { get; init; } = [];
    public IReadOnlyList<BrandFilterItem> Brands { get; init; } = [];
    public decimal MinPrice { get; init; }
    public decimal MaxPrice { get; init; }
}
