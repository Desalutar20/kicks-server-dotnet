using Application.Admin.Products.ProductSkus.UseCases.GetProductSku;
using Application.Auth.Types;
using Domain.Product.ProductSku;
using Presentation.Admin.Products.ProductSkus.Dto;
using Presentation.Shared;

namespace Presentation.Admin.Products.ProductSkus.Endpoints;

internal static partial class AdminProductSkusEndpoints
{
    private static IEndpointRouteBuilder GetProductSkuV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/skus/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    IQueryHandler<GetProductSkuQuery, ProductSku> queryHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.GetProductSku");

                    var query = new GetProductSkuQuery(new ProductSkuId(id));
                    var result = await queryHandler.Handle(query, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(new ApiResponse<AdminProductSkuDto>(result.Value.ToDto()));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<AdminProductSkuDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetProductSku")
            .WithSummary("Get product SKU by ID")
            .WithDescription(
                "Fetches a single product SKU including product details. Admin access required."
            );

        return endpoint;
    }
}
