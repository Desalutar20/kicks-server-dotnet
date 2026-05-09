using Domain.Product.Brand;
using Domain.Product.Category;
using Domain.Shared;

namespace Domain.Product;

public sealed record ProductFilters(
    NonEmptyString? Search,
    ProductGender? Gender,
    BrandId? BrandId,
    CategoryId? CategoryId,
    bool? IsDeleted
);
