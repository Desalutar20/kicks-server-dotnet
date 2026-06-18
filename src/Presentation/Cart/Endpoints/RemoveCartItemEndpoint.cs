using Application.Auth.Types;
using Application.Carts.UseCases.RemoveCartItem;
using Domain.Products.ProductSkus;
using Presentation.Shared.Extensions;

namespace Presentation.Cart.Endpoints;

internal static partial class CartEndpoints
{
    private static IEndpointRouteBuilder RemoveCartItemV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapDelete(
                "/items/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<RemoveCartItemCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("RemoveCartItem");

                    var command = new RemoveCartItemCommand(sessionUser.Id, new ProductSkuId(id));
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
            .WithName("RemoveCartItem")
            .WithSummary("Remove an item from the current user's cart")
            .WithDescription("Removes a product SKU from the authenticated user's shopping cart.")
            .RequireRateLimiting(RateLimitConstants.RemoveCartItem);

        return endpoint;
    }
}
