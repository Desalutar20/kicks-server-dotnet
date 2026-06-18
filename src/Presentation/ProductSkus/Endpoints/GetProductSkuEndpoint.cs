using Application.ProductSkus.Types;
using Application.ProductSkus.UseCases.GetProductSku;
using Domain.Products.ProductSkus;
using Presentation.ProductSkus.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.ProductSkus.Endpoints;

internal static partial class CartEndpoints
{
    private static IEndpointRouteBuilder GetProductSkuV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetProductSkuQuery, ProductSkuWithVariants> queryHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("GetProductSku");

                    var query = new GetProductSkuQuery(new ProductSkuId(id));
                    var result = await queryHandler.Handle(query, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(
                            new ApiResponse<ProductSkuWithVariantsDto>(result.Value.ToDto())
                        );
                }
            )
            .Produces<ApiResponse<ProductSkuWithVariantsDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProductSku")
            .WithSummary("Get product SKU and it's variants by ID")
            .WithDescription("Fetches a single product SKU including with variants.")
            .RequireRateLimiting(RateLimitConstants.GetProductSku);

        return endpoint;
    }
}
