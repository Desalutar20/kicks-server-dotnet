using Application.Admin.Products.ProductSkus.UseCases.GetAdminProductSku;
using Application.Auth.Types;
using Domain.Products.ProductSkus;
using Presentation.Admin.Products.ProductSkus.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.Products.ProductSkus.Endpoints;

internal static partial class AdminProductSkusEndpoints
{
    private static IEndpointRouteBuilder GetAdminProductSkuV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/skus/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    IQueryHandler<GetAdminProductSkuQuery, ProductSku> queryHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.GetAdminProductSku");

                    var query = new GetAdminProductSkuQuery(new ProductSkuId(id));
                    var result = await queryHandler.Handle(query, ct);
                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
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
            .WithName("GetAdminProductSku")
            .WithSummary("Get admin product SKU by ID")
            .WithDescription(
                "Fetches a single product SKU including product details. Admin access required."
            );

        return endpoint;
    }
}
