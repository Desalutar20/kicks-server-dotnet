namespace Application.Admin.Brands.Errors;

internal static class AdminBrandErrors
{
    public static Result BrandNotFound(BrandId brandId) =>
        Result.Failure(Error.Failure($"Brand with id '{brandId}' doesn't exist"));

    public static Error BrandAlreadyExists(BrandName name) =>
        Error.Failure($"Brand with name '{name}' already exists");
}
