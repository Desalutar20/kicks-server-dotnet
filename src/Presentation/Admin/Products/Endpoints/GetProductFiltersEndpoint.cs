using Application.Admin.Products.UseCases.GetProductFilters;
using Application.Auth.Types;
using Domain.Product;
using Presentation.Admin.Products.Dto;
using Presentation.Shared;

namespace Presentation.Admin.Products.Endpoints;

internal static partial class AdminProductSkusEndpoints
{
    private static IEndpointRouteBuilder GetProductFiltersV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/filters",
                async (
                    HttpContext ctx,
                    IQueryHandler<GetProductFiltersQuery, ProductFilterOptions> queryHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.GetProductFilters");

                    var result = await queryHandler.Handle(new GetProductFiltersQuery(), ct);
                    if (result.IsFailure)
                    {
                        return ErrorHandler.Handle(result.Error, logger);
                    }

                    return Results.Ok(
                        new ApiResponse<ProductFilterOptionsDto>(result.Value.ToDto())
                    );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .Produces<ApiResponse<ProductFilterOptionsDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetProductFilters")
            .WithSummary("Retrieves available product filter options for the admin panel.")
            .WithDescription(
                "Returns all available filter options for products, including categories, brands, tags, and their available values used in the admin panel product filtering system."
            );

        return endpoint;
    }
}
