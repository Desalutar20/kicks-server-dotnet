namespace Application.Admin.Products.Errors;

public static class AdminProductErrors
{
    public static Error BrandDoesNotExist(BrandId brandId) =>
        Error.Failure($"Brand with id '{brandId}' doesn't exist");

    public static Error CategoryDoesNotExist(CategoryId categoryId) =>
        Error.Failure($"Category with id '{categoryId}' doesn't exist");

    public static Error ProductAlreadyExists(
        ProductTitle title,
        ProductGender gender,
        BrandId? brandId,
        CategoryId? categoryId
    ) =>
        Error.Failure(
            $"Product with title '{title}', gender '{gender}', brandId '{brandId}', categoryId '{categoryId}' already exists"
        );

    public static Error ProductNotFound(ProductId productId) =>
        Error.Failure($"Product with id '{productId}' doesn't exist");
}
