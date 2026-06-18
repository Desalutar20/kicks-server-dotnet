using Application.Auth.Types;
using Application.Carts.UseCases.UpdateCartItemQuantity;
using Domain.Products.ProductSkus;
using Domain.Shared.ValueObjects;
using Presentation.Shared.Extensions;

namespace Presentation.Cart.Endpoints;

public sealed record UpdateCartItemQuantityRequest(int Quantity);

public sealed class UpdateCartItemQuantityRequestValidator
    : AbstractValidator<UpdateCartItemQuantityRequest>
{
    public UpdateCartItemQuantityRequestValidator()
    {
        RuleFor(x => x.Quantity).ValidateValueObject(x => PositiveInt.Create(x, label: "Quantity"));
    }
}

internal static partial class OrderEndpoints
{
    private static IEndpointRouteBuilder UpdateCartItemQuantityV1(
        this IEndpointRouteBuilder endpoint
    )
    {
        endpoint
            .MapPatch(
                "/items/{id:guid}",
                async (
                    Guid id,
                    UpdateCartItemQuantityRequest request,
                    ICommandHandler<UpdateCartItemQuantityCommand> commandHandler,
                    HttpContext ctx,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var user)
                        || user is not SessionUser sessionUser
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var logger = loggerFactory.CreateLogger("UpdateCartItemQuantity");

                    var command = request.ToCommand(sessionUser.Id, id);
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(new ApiResponse<string>("success"));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("UpdateCartItemQuantity")
            .WithSummary("Update cart item quantity")
            .WithDescription(
                "Updates the quantity of a product SKU in the authenticated user's shopping cart."
            )
            .RequireRateLimiting(RateLimitConstants.UpdateCartItemQuantity);

        return endpoint;
    }

    private static UpdateCartItemQuantityCommand ToCommand(
        this UpdateCartItemQuantityRequest request,
        UserId userId,
        Guid productSkuId
    )
    {
        var quantity = PositiveInt.Create(request.Quantity).Value;

        return new UpdateCartItemQuantityCommand(userId, new ProductSkuId(productSkuId), quantity);
    }
}
