using Domain.Brands;
using Domain.Categories;
using Domain.Shared;
using Domain.Shared.ValueObjects;

namespace Domain.Products.ProductSkus;

public sealed record ProductSkusFilters(
    IEnumerable<PositiveInt>? Sizes,
    IEnumerable<ProductSkuColor>? Colors,
    IEnumerable<CategoryId>? CategoryIds,
    IEnumerable<BrandId>? BrandIds,
    IEnumerable<ProductGender>? Genders,
    Money? MinPrice,
    Money? MaxPrice
);
