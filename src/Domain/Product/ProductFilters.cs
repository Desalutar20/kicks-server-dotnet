using Domain.Brand;
using Domain.Category;
using Domain.Shared;

namespace Domain.Product;

public sealed record ProductFilters(
    NonEmptyString? Search,
    ProductGender? Gender,
    BrandId? BrandId,
    CategoryId? CategoryId,
    bool? IsDeleted
);
