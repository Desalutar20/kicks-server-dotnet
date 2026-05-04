using Domain.Product.Brand;

namespace Presentation.Admin.Brands.Dto;

public sealed record BrandDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Name
);

internal static class BrandDtoMapper
{
    public static BrandDto ToDto(this Brand brand) =>
        new(brand.Id.Value, brand.CreatedAt, brand.UpdatedAt, brand.Name.ToString());
}
