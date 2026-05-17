using Domain.Brand;

namespace Presentation.Admin.Brands.Dto;

public sealed record AdminBrandDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Name
);

internal static class AdminBrandDtoMapper
{
    public static AdminBrandDto ToDto(this Brand brand) =>
        new(brand.Id.Value, brand.CreatedAt, brand.UpdatedAt, brand.Name.ToString());
}
