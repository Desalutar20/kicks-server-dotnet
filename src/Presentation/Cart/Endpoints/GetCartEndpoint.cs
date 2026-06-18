using Application.Auth.Types;
using Application.Carts.UseCases.GetCart;
using Presentation.Cart.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.Cart.Endpoints;

internal static partial class CartEndpoints
{
    private static IEndpointRouteBuilder GetCartV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "",
                async (
                    IQueryHandler<GetCartQuery, Domain.Carts.Cart> queryHandler,
                    HttpContext ctx,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var user)
                        || user is not SessionUser sessionUser
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var logger = loggerFactory.CreateLogger("GetCart");

                    var query = new GetCartQuery(sessionUser.Id);
                    var result = await queryHandler.Handle(query, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(new ApiResponse<CartDto>(result.Value.ToDto()));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .Produces<ApiResponse<CartDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetCart")
            .WithSummary("Get current user's cart")
            .WithDescription(
                "Retrieves the shopping cart for the authenticated user, including all cart items."
            )
            .RequireRateLimiting(RateLimitConstants.GetCart);

        return endpoint;
    }
}
