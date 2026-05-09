namespace Application.Admin.Categories.Errors;

internal static class AdminCategoryErrors
{
    public static Result CategoryNotFound(CategoryId categoryId) =>
        Result.Failure(Error.Failure($"Category with id '{categoryId}' doesn't exist"));

    public static Error CategoryAlreadyExists(CategoryName name) =>
        Error.Failure($"Category with name '{name}' already exists");
}
