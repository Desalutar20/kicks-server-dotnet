using Domain.Product.Category;

namespace Application.Admin.Categories.Errors;

internal static class AdminCategoriesErrors
{
    public static Result CategoryNotFound(CategoryId caegoryId) =>
        Result.Failure(Error.Failure($"Caegory with id '{caegoryId}' doesn't exist"));

    public static Error CategoryAlreadyExists(CategoryName name) =>
        Error.Failure($"Caegory with name '{name}' already exists");
}
