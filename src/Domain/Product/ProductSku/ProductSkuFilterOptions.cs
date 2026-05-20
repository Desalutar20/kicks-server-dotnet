using Domain.Shared;

namespace Domain.Product.ProductSku;

public sealed record ProductSkusFilterOptions(
    IEnumerable<int> Sizes,
    IEnumerable<string> Colors,
    IEnumerable<ProductGender> Genders,
    IEnumerable<CategoryFilterItem> Categories,
    IEnumerable<BrandFilterItem> Brands,
    PositiveInt MinPrice,
    PositiveInt MaxPrice
);
