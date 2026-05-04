using Application.Auth.Types;
using Presentation.Auth.Dto;

namespace Presentation.Auth.Endpoints;

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder GetProfileV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/profile",
                (HttpContext ctx) =>
                {
                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var result)
                        || result is not SessionUser sessionUser
                    )
                    {
                        return Results.Unauthorized();
                    }

                    return Results.Ok(new ApiResponse<UserDto>(sessionUser.ToDto()));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .Produces<ApiResponse<UserDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProfile")
            .WithSummary("Retrieves the current authenticated user's profile.")
            .WithDescription(
                "Returns the profile information of the user based on the authentication token."
            )
            .RequireRateLimiting(RateLimitConstants.GetProfile);

        return endpoint;
    }
}
