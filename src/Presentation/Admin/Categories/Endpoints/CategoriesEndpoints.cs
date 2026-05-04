namespace Presentation.Admin.Categories.Endpoints;

internal static partial class AdminCategoriesEndpoints
{
    public static void MapCategoriesV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/categories").WithTags("Admin categories");

        group.GetCategoriesV1().CreateCategoryV1().UpdateCategoryV1().DeleteCategoryV1();
    }
}
