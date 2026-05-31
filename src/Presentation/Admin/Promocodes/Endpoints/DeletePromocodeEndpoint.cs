using Application.Admin.Promocodes.UseCases.DeletePromocode;
using Application.Auth.Types;
using Domain.Promocodes;

namespace Presentation.Admin.Promocodes.Endpoints;

internal static partial class AdminPromocodesEndpoints
{
    private static IEndpointRouteBuilder DeletePromocodeV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapDelete(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    ICommandHandler<DeletePromocodeCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.DeletePromocode");

                    var command = new DeletePromocodeCommand(new PromocodeId(id));
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
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
            .WithName("DeletePromocode")
            .WithSummary("Delete promocode")
            .WithDescription(
                "Permanently deletes a promocode by their unique identifier. This action cannot be undone."
            );

        return endpoint;
    }
}
