using Domain.Product;

namespace Presentation.Admin.Products.Dto;

public sealed record ProductFilterOptionsDto(
    List<string> Tags,
    List<CategoryItemDto> Categories,
    List<CategoryItemDto> AvailableCategories,
    List<BrandItemDto> Brands,
    List<BrandItemDto> AvailableBrands
);

public sealed record CategoryItemDto(Guid Id, string Name);

public sealed record BrandItemDto(Guid Id, string Name);

internal static class ProductFilterOptionsDtoMapper
{
    public static ProductFilterOptionsDto ToDto(this ProductFilterOptions model) =>
        new(
            model.Tags.ToList(),
            model.Categories.Select(x => new CategoryItemDto(x.Id.Value, x.Name.Value)).ToList(),
            model
                .AvailableCategories.Select(x => new CategoryItemDto(x.Id.Value, x.Name.Value))
                .ToList(),
            model.Brands.Select(x => new BrandItemDto(x.Id.Value, x.Name.Value)).ToList(),
            model.AvailableBrands.Select(x => new BrandItemDto(x.Id.Value, x.Name.Value)).ToList()
        );
}
