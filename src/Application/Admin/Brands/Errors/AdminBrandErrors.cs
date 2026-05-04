using Domain.Product.Brand;

namespace Application.Admin.Brands.Errors;

internal static class AdminBrandErrors
{
    public static Result BrandNotFound(BrandId brandId) =>
        Result.Failure(Error.Failure($"Brand with id '{brandId}' doesn't exist"));

    public static Result BrandAlreadyExists(BrandName name) =>
        Result.Failure(Error.Failure($"Brand with name '{name}' already exists"));
}
