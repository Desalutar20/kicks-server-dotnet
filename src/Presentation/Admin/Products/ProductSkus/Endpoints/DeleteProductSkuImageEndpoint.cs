using Application.Admin.Products.ProductSkus.UseCases.DeleteProductSku;
using Application.Admin.Products.ProductSkus.UseCases.DeleteProductSkuImage;
using Application.Auth.Types;
using Domain.Product.ProductSku;
using Domain.Product.ProductSku.ProductSkuImage;
using Presentation.Shared;

namespace Presentation.Admin.Products.ProductSkus.Endpoints;

internal static partial class AdminProductSkusEndpoints
{
    private static IEndpointRouteBuilder DeleteProductSkuImageV1(
        this IEndpointRouteBuilder endpoint
    )
    {
        endpoint
            .MapDelete(
                "/skus/{id:guid}/images/{imageId:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    Guid imageId,
                    ICommandHandler<DeleteProductSkuImageCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.DeleteProductSkuImage");

                    var command = new DeleteProductSkuImageCommand(
                        new ProductSkuId(id),
                        new ProductSkuImageId(imageId)
                    );
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
            .WithName("DeleteProductSkuImage")
            .WithSummary("Delete product sku image")
            .WithDescription(
                "Permanently deletes a product sku image by their unique identifier. This action cannot be undone."
            );

        return endpoint;
    }
}
