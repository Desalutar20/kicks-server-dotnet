using Application.Auth.Types;
using Application.Carts.UseCases.RemovePromocode;
using Presentation.Shared.Extensions;

namespace Presentation.Cart.Endpoints;

internal static partial class OrderEndpoints
{
    private static IEndpointRouteBuilder RemovePromocodeV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapDelete(
                "/promocode",
                async (
                    ICommandHandler<RemovePromocodeCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("RemovePromocode");

                    var command = new RemovePromocodeCommand(sessionUser.Id);
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
            .WithName("RemovePromocode")
            .WithSummary("Remove promocode")
            .WithDescription(
                "Removes the applied promocode from the authenticated user's shopping cart."
            )
            .RequireRateLimiting(RateLimitConstants.RemovePromocode);

        return endpoint;
    }
}
