namespace Application.Admin.Products.ProductSkus.Constants;

public static class ProductSkusConstants
{
    public const int GetAdminProductSkusDefaultLimit = 40;
    public const int GetAdminProductSkusMaxLimit = 100;

    public const int GetProductSkusDefaultLimit = 30;
    public const int GetProductSkusMaxLimit = 50;
    public const int GetProductSkusCursorMaxLength = 100;
    public static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;
}
