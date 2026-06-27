using Application.Shared.Types;

namespace Application.Admin.Products.Types;

public sealed record ProductFilterOptions
{
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<CategoryFilterItem> Categories { get; init; } = [];
    public IReadOnlyList<CategoryFilterItem> AvailableCategories { get; init; } = [];
    public IReadOnlyList<BrandFilterItem> Brands { get; init; } = [];
    public IReadOnlyList<BrandFilterItem> AvailableBrands { get; init; } = [];
};
