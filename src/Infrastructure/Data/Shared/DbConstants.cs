namespace Infrastructure.Data.Shared;

internal static class DbConstants
{
    public const string UniqueViolationErrorCode = "23505";
    public const string ForeignKeyViolationErrorCode = "23503";

    public const string BrandUniqueIndex = "uq_brand_name";
    public const string CategoryUniqueIndex = "uq_category_name";

    public const string ProductUniqueIndex = "uq_product_title_gender_category_brand";
    public const string ProductSkuDuplicateCombinationUniqueIndex =
        "uq_product_sku_product_size_color";
    public const string ProductSkuSkuUniqueIndex = "uq_product_sku_sku";
}
