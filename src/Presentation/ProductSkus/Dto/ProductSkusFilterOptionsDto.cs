using Domain.Products;
using Domain.Products.ProductSkus;

namespace Presentation.ProductSkus.Dto;

public sealed record ProductSkusFilterOptionsDto(
    List<int> Sizes,
    List<string> Colors,
    List<ProductGender> Genders,
    List<CategoryItemDto> Categories,
    List<BrandItemDto> Brands,
    decimal MinPrice,
    decimal MaxPrice
);

internal static class ProductSkusFilterOptionsDtoMapper
{
    public static ProductSkusFilterOptionsDto ToDto(this ProductSkusFilterOptions model) =>
        new(
            model.Sizes.ToList(),
            model.Colors.ToList(),
            model.Genders.ToList(),
            model.Categories.Select(x => new CategoryItemDto(x.Id.Value, x.Name.Value)).ToList(),
            model.Brands.Select(x => new BrandItemDto(x.Id.Value, x.Name.Value)).ToList(),
            model.MinPrice.Dollars,
            model.MaxPrice.Dollars
        );
}
