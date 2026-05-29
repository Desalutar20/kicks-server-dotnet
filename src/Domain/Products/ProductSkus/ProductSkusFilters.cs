using Domain.Brands;
using Domain.Categories;
using Domain.Shared;

namespace Domain.Products.ProductSkus;

public sealed record ProductSkusFilters(
    IEnumerable<PositiveInt>? Sizes,
    IEnumerable<ProductSkuColor>? Colors,
    IEnumerable<CategoryId>? CategoryIds,
    IEnumerable<BrandId>? BrandIds,
    IEnumerable<ProductGender>? Genders,
    PositiveInt? MinPrice,
    PositiveInt? MaxPrice
);
