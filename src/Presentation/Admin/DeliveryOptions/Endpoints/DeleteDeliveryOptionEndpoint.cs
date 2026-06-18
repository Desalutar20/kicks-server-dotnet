using Application.Admin.DeliveryOptions.UseCases.DeleteDeliveryOption;
using Application.Auth.Types;
using Domain.DeliveryOptions;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.DeliveryOptions.Endpoints;

internal static partial class DeliveryOptionsEndpoints
{
    private static IEndpointRouteBuilder DeleteDeliveryOptionV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapDelete(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    ICommandHandler<DeleteDeliveryOptionCommand> commandHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var user)
                        || user is not SessionUser
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var logger = loggerFactory.CreateLogger("Admin.DeleteDeliveryOption");

                    var command = new DeleteDeliveryOptionCommand(new DeliveryOptionId(id));
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(new ApiResponse<string>("Success"));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("DeleteDeliveryOption")
            .WithSummary("Delete delivery option")
            .WithDescription(
                "Permanently deletes a delivery option by their unique identifier. This action cannot be undone."
            );

        return endpoint;
    }
}
