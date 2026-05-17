using Domain.Product.ProductSku;

namespace Application.Admin.Products.ProductSkus.Errors;

public static class AdminProductSkuErrors
{
    public static Error ProductSkuDuplicateCombination(
        ProductId productId,
        ProductSkuColor color,
        PositiveInt size
    ) =>
        Error.Failure(
            $"Product sku with product id '{productId}', color '{color}',  size '{size}' already exists"
        );

    public static Error ProductSkuAlreadyExists(ProductSkuSku sku) =>
        Error.Failure($"Product sku with sku'{sku}' already exists");

    public static Error ProductSkuNotFound(ProductSkuId productSkuId) =>
        Error.Failure($"Product sku with id '{productSkuId}' doesn't exist");
}
