namespace Domain.Product;

public sealed record ProductFilterOptions(
    IEnumerable<string> Tags,
    IEnumerable<CategoryFilterItem> Categories,
    IEnumerable<CategoryFilterItem> AvailableCategories,
    IEnumerable<BrandFilterItem> Brands,
    IEnumerable<BrandFilterItem> AvailableBrands
);
