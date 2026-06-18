using Application.Auth.Types;
using Application.Carts.UseCases.ClearCart;
using Presentation.Shared.Extensions;

namespace Presentation.Cart.Endpoints;

internal static partial class OrderEndpoints
{
    private static IEndpointRouteBuilder ClearCartV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapDelete(
                "/items",
                async (
                    ICommandHandler<ClearCartCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("ClearCart");

                    var command = new ClearCartCommand(sessionUser.Id);
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(new ApiResponse<string>("success"));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("ClearCart")
            .WithSummary("Clear shopping cart")
            .WithDescription("Removes all items from the authenticated user's shopping cart.")
            .RequireRateLimiting(RateLimitConstants.ClearCart);

        return endpoint;
    }
}
