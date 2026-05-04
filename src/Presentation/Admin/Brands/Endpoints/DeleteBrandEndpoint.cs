using Application.Admin.Brands.UseCases.DeleteBrand;
using Application.Auth.Types;
using Domain.Product.Brand;

namespace Presentation.Admin.Brands.Endpoints;

internal static partial class AdminBrandsEndpoints
{
    private static IEndpointRouteBuilder DeleteBrandV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapDelete(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    ICommandHandler<DeleteBrandCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.DeleteBrand");

                    var command = new DeleteBrandCommand(new BrandId(id));
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
            .WithName("DeleteBrand")
            .WithSummary("Delete brand")
            .WithDescription(
                "Permanently deletes a brand by their unique identifier. This action cannot be undone."
            );

        return endpoint;
    }
}
