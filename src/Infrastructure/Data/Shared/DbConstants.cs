namespace Infrastructure.Data.Shared;

internal static class DbConstants
{
    public const string UniqueViolationErrorCode = "23505";
    public const string ForeignKeyViolationErrorCode = "23503";

    public const string UserUniqueIndex = "uq_user_email";

    public const string BrandUniqueIndex = "uq_brand_name";
    public const string CategoryUniqueIndex = "uq_category_name";

    public const string ProductUniqueIndex = "uq_product_title_gender_category_brand";
    public const string ProductSkuDuplicateCombinationUniqueIndex =
        "uq_product_sku_product_size_color";
    public const string ProductSkuSkuUniqueIndex = "uq_product_sku_sku";

    public const string ProductSkuReviewUniqueUserPerSkuIndex =
        "uq_product_sku_review_product_sku_user";

    public const string PromocodeUniqueIndex = "uq_promocode_code";

    public const string ProductBrandForeignKey = "fk_product_brand_brand_id";
    public const string ProductCategoryForeignKey = "fk_product_category_category_id";
}
