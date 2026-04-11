using Application.Config;

namespace Presentation.Auth.Endpoints;

internal static partial class AuthEndpoint
{
    public static IEndpointRouteBuilder MapAuthV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/auth")
                          .WithTags("Authentication");

        group
            .SignUpV1()
            .SignInV1()
            .VerifyAccountV1()
            .ForgotPasswordV1()
            .ResetPasswordV1()
            .GetProfileV1()
            .LogoutV1();

        return group;
    }

    private static void SetSessionCookie(HttpContext ctx, Guid sessionId, ApplicationConfig config)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(config.SessionTtlMinutes),
            HttpOnly = true,
            Secure = config.CookieSecure,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };

        ctx.Response.Cookies.Append(config.SessionCookieName,
            sessionId.ToString(), cookieOptions);
    }

    private static void ClearSessionCookie(HttpContext ctx, ApplicationConfig config)
    {
        var cookieOptions =
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(-1), Path = "/" };
        ctx.Response.Cookies.Append(config.SessionCookieName, "",
            cookieOptions);
    }
}