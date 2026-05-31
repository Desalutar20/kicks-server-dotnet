namespace Presentation.Admin.Promocodes.Endpoints;

internal static partial class AdminPromocodesEndpoints
{
    public static void MapAdminPromocodesV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/promocodes").WithTags("Admin promocodes");

        group.GetPromocodesV1().CreatePromocodeV1().UpdatePromocodeV1().DeletePromocodeV1();
    }
}
