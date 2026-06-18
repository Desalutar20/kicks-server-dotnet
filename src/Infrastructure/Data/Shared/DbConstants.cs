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
    public const string CartUniqueIndex = "uq_cart_user";
    public const string CartItemUniqueIndex = "uq_cart_items_cart_id_product_sku_id";
    public const string OrderItemUniqueIndex = "uq_order_items_order_id_product_sku_id";
    public const string OrderUserPromocodeUniqueIndex = "uq_order_user_promocode";
    public const string OrderUserPendingUniqueIndex = "uq_order_user_pending_unique";

    public const string ProductBrandForeignKey = "fk_product_brand_brand_id";
    public const string ProductCategoryForeignKey = "fk_product_category_category_id";
    public const string ProductSkuProductForeignKey = "fk_product_sku_product_product_id";
    public const string CartItemProductSkuForeignKey = "fk_cart_item_product_sku_product_sku_id";
    public const string OrderDeliveryOptionForeignKey =
        "fk_order_delivery_option_delivery_option_id";
}
