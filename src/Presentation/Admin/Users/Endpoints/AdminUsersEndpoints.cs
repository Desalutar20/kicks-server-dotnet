namespace Presentation.Admin.Users.Endpoints;

internal static partial class AdminUsersEndpoints
{
    public static void MapAdminUsersV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/users").WithTags("Admin users");

        group.GetAdminUsersV1().ToggleBanUserV1().DeleteUserV1();
    }
}
