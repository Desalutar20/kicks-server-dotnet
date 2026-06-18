using Application.Auth.Types;
using Application.Carts.UseCases.AddCartItem;
using Domain.Products.ProductSkus;
using Domain.Shared.ValueObjects;
using Presentation.Shared.Extensions;

namespace Presentation.Cart.Endpoints;

public sealed record AddCartItemRequest(string ProductSkuId, int Quantity);

public sealed class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.ProductSkuId)
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage("Invalid product sku id");

        RuleFor(x => x.Quantity).ValidateValueObject(x => PositiveInt.Create(x, label: "Quantity"));
    }
}

internal static partial class CartEndpoints
{
    private static IEndpointRouteBuilder AddCartItemV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/items",
                async (
                    AddCartItemRequest request,
                    ICommandHandler<AddCartItemCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("AddCartItem");

                    var command = request.ToCommand(sessionUser.Id);
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
            .WithName("AddCartItem")
            .WithSummary("Add an item to the current user's cart")
            .WithDescription(
                "Adds a product SKU to the authenticated user's shopping cart. "
                    + "If the item already exists in the cart, its quantity is increased by the specified amount."
            )
            .RequireRateLimiting(RateLimitConstants.AddCartItem);

        return endpoint;
    }

    private static AddCartItemCommand ToCommand(this AddCartItemRequest request, UserId userId)
    {
        var productSkuId = new ProductSkuId(Guid.Parse(request.ProductSkuId));
        var quantity = PositiveInt.Create(request.Quantity).Value;

        return new AddCartItemCommand(userId, productSkuId, quantity);
    }
}
