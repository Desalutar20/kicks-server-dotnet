using Domain.Brand;
using Domain.Category;
using Domain.Shared;

namespace Domain.Product.ProductSku;

public sealed record ProductSkusFilters(
    IEnumerable<PositiveInt>? Sizes,
    IEnumerable<ProductSkuColor>? Colors,
    IEnumerable<CategoryId>? CategoryIds,
    IEnumerable<BrandId>? BrandIds,
    IEnumerable<ProductGender>? Genders,
    PositiveInt? MinPrice,
    PositiveInt? MaxPrice
);
