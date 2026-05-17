using Application.Admin.Products.UseCases.DeleteProduct;
using Application.Auth.Types;
using Domain.Product;
using Presentation.Shared;

namespace Presentation.Admin.Products.Endpoints;

internal static partial class AdminProductSkusEndpoints
{
    private static IEndpointRouteBuilder ToggleProductIsDeletedV1(
        this IEndpointRouteBuilder endpoint
    )
    {
        endpoint
            .MapPost(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    ICommandHandler<ToggleProductIsDeletedCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.ToggleProductIsDeleted");

                    var command = new ToggleProductIsDeletedCommand(new ProductId(id));
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
            .WithName("ToggleProductIsDeleted")
            .WithSummary("Toggle product deletion status")
            .WithDescription(
                "Toggles the product's deletion state. If the product is active, it will be marked as deleted (soft delete). If it is already deleted, it will be restored."
            );

        return endpoint;
    }
}
