using Application.Admin.Categories.UseCases.DeleteCategory;
using Application.Auth.Types;
using Domain.Product.Category;

namespace Presentation.Admin.Categories.Endpoints;

internal static partial class AdminCategoriesEndpoints
{
    private static IEndpointRouteBuilder DeleteCategoryV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapDelete(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    ICommandHandler<DeleteCategoryCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.DeleteCategory");

                    var command = new DeleteCategoryCommand(new CategoryId(id));
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
            .WithName("DeleteCategory")
            .WithSummary("Delete category")
            .WithDescription(
                "Permanently deletes a category by their unique identifier. This action cannot be undone."
            );

        return endpoint;
    }
}
