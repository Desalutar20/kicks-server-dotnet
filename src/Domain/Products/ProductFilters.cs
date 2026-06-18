using Domain.Brands;
using Domain.Categories;
using Domain.Shared;
using Domain.Shared.ValueObjects;

namespace Domain.Products;

public sealed record ProductFilters(
    NonEmptyString? Search,
    ProductGender? Gender,
    BrandId? BrandId,
    CategoryId? CategoryId,
    bool? IsDeleted
);
