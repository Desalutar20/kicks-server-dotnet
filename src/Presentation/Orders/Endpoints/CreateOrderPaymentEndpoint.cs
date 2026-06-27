using Application.Auth.Types;
using Application.Orders.UseCases.CreateOrderPayment;
using Domain.Orders;
using Presentation.Shared.Extensions;

namespace Presentation.Orders.Endpoints;

internal static partial class OrderEndpoints
{
    private static IEndpointRouteBuilder CreateOrderPaymentV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/{id:guid}/payments",
                async (
                    Guid id,
                    ICommandHandler<CreateOrderPaymentCommand, Uri> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("CreateOrderPayment");

                    var command = new CreateOrderPaymentCommand(sessionUser.Id, new OrderId(id));
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(new ApiResponse<Uri>(result.Value));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<Uri>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("CreateOrderPayment")
            .WithSummary("Get order by id")
            .WithDescription(
                "Retrieves a specific order belonging to the authenticated user by its identifier."
            )
            .RequireRateLimiting(RateLimitConstants.GetOrder);

        return endpoint;
    }
}
