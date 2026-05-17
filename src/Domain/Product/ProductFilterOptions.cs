using Domain.Brand;
using Domain.Category;

namespace Domain.Product;

public sealed record ProductFilterOptions(
    IEnumerable<string> Tags,
    IEnumerable<CategoryFilterItem> Categories,
    IEnumerable<CategoryFilterItem> AvailableCategories,
    IEnumerable<BrandFilterItem> Brands,
    IEnumerable<BrandFilterItem> AvailableBrands
);

public sealed record CategoryFilterItem(CategoryId Id, CategoryName Name);

public sealed record BrandFilterItem(BrandId Id, BrandName Name);
