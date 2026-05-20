using Application.ProductSkus.UseCases.GetProductSkusFilters;
using Domain.Product.ProductSku;
using Presentation.ProductSkus.Dto;
using Presentation.Shared;

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
                        return ErrorHandler.Handle(result.Error, logger);
                    }

                    return Results.Ok(
                        new ApiResponse<ProductSkusFilterOptionsDto>(result.Value.ToDto())
                    );
                }
            )
            .Produces<ApiResponse<ProductSkusFilterOptionsDto>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProductSkusFilters")
            .WithSummary("Retrieves available product skus filter options.")
            .WithDescription(
                "Returns all available filter options for product skus, including categories, brands, sizes, colors, minPrice and maxPrice."
            );

        return endpoint;
    }
}
