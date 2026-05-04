namespace Presentation.Admin.Brands.Endpoints;

internal static partial class AdminBrandsEndpoints
{
    public static void MapBrandsV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/brands").WithTags("Admin brands");

        group.GetBrandsV1().CreateBrandV1().UpdateBrandV1().DeleteBrandV1();
    }
}
