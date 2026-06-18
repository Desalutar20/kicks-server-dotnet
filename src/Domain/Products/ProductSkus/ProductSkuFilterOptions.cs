using Domain.Shared.ValueObjects;

namespace Domain.Products.ProductSkus;

public sealed record ProductSkusFilterOptions(
    IEnumerable<int> Sizes,
    IEnumerable<string> Colors,
    IEnumerable<ProductGender> Genders,
    IEnumerable<CategoryFilterItem> Categories,
    IEnumerable<BrandFilterItem> Brands,
    Money MinPrice,
    Money MaxPrice
);
