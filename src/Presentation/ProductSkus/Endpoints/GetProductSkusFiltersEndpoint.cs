using Application.ProductSkus.Types;
using Application.ProductSkus.UseCases.GetProductSkusFilters;
using Presentation.Shared.Extensions;

namespace Presentation.ProductSkus.Endpoints;

internal static partial class ProductSkusEndpoints
{
    private static IEndpointRouteBuilder GetProductSkusFiltersV1(
        this IEndpointRouteBuilder endpoint
    )
    {
        endpoint
            .MapGet(
                "/filters",
                async (
                    IQueryHandler<
                        GetProductSkusFiltersQuery,
                        ProductSkusFilterOptions
                    > queryHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("GetProductSkusFilters");

                    var result = await queryHandler.Handle(new GetProductSkusFiltersQuery(), ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Ok(new ApiResponse<ProductSkusFilterOptions>(result.Value));
                }
            )
            .Produces<ApiResponse<ProductSkusFilterOptions>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProductSkusFilters")
            .WithSummary("Retrieves available product skus filter options.")
            .WithDescription(
                "Returns all available filter options for product skus, including categories, brands, sizes, colors, minPrice and maxPrice."
            )
            .RequireRateLimiting(RateLimitConstants.GetProductSkusFilters);

        return endpoint;
    }
}
