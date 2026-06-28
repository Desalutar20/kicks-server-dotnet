using Application.Auth.Types;
using Application.Orders.Types;
using Application.Orders.UseCases.GetOrder;
using Domain.Orders;
using Presentation.Shared.Extensions;

namespace Presentation.Orders.Endpoints;

internal static partial class OrderEndpoints
{
    private static IEndpointRouteBuilder GetOrderV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetOrderQuery, OrderResponse> queryHandler,
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

                    var logger = loggerFactory.CreateLogger("GetOrder");

                    var query = new GetOrderQuery(new UserId(sessionUser.Id), new OrderId(id));
                    var result = await queryHandler.Handle(query, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(new ApiResponse<OrderResponse>(result.Value));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<OrderResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetOrder")
            .WithSummary("Get order by id")
            .WithDescription(
                "Retrieves a specific order belonging to the authenticated user by its identifier."
            )
            .RequireRateLimiting(RateLimitConstants.GetOrder);

        return endpoint;
    }
}
