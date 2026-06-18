using Application.Auth.Types;
using Application.Orders.UseCases.CreateOrder;
using Domain.DeliveryOptions;
using Domain.Orders;
using Domain.Shared.ValueObjects;
using Presentation.Orders.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.Orders.Endpoints;

public sealed record CreateOrderRequest(
    string Email,
    string PhoneNumber,
    OrderAddressDto? BillingAddress,
    OrderAddressDto DeliveryAddress,
    string DeliveryOptionId
);

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Email).ValidateValueObject(Email.Create);
        RuleFor(x => x.PhoneNumber)
            .ValidateValueObject(v => NonEmptyString.Create(v, label: "Phone number"));

        RuleFor(x => x.BillingAddress)
            .ValidateNullableValueObject(v =>
                OrderAddress.Create(v.City, v.Street, v.Home, v.Apartment)
            );

        RuleFor(x => x.DeliveryAddress)
            .Must(x => x is not null)
            .ValidateValueObject(v => OrderAddress.Create(v.City, v.Street, v.Home, v.Apartment));

        RuleFor(x => x.DeliveryOptionId)
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage("Invalid delivery option id");
    }
}

internal static partial class OrderEndpoints
{
    private static IEndpointRouteBuilder CreateOrderV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "",
                async (
                    CreateOrderRequest request,
                    ICommandHandler<CreateOrderCommand, OrderId> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("CreateOrder");

                    var command = request.ToCommand(sessionUser.Id);
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.CreatedAtRoute(
                            "GetOrder",
                            new { id = result.Value },
                            new ApiResponse<Guid>(result.Value)
                        );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<Guid>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("CreateOrder")
            .WithSummary("Checkout shopping cart and create order")
            .WithDescription(
                "Creates an order from the authenticated user's cart using provided delivery and billing information. Billing address is optional."
            )
            .RequireRateLimiting(RateLimitConstants.CreateOrder);

        return endpoint;
    }

    private static CreateOrderCommand ToCommand(this CreateOrderRequest request, UserId userId)
    {
        var email = Email.Create(request.Email).Value;
        var phoneNumber = NonEmptyString.Create(request.PhoneNumber).Value;
        var billingAddress = request.BillingAddress is not null
            ? OrderAddress
                .Create(
                    request.BillingAddress.City,
                    request.BillingAddress.Street,
                    request.BillingAddress.Home,
                    request.BillingAddress.Apartment
                )
                .Value
            : null;

        var deliveryAddress = OrderAddress
            .Create(
                request.DeliveryAddress.City,
                request.DeliveryAddress.Street,
                request.DeliveryAddress.Home,
                request.DeliveryAddress.Apartment
            )
            .Value;

        return new CreateOrderCommand(
            userId,
            email,
            phoneNumber,
            billingAddress,
            deliveryAddress,
            new DeliveryOptionId(Guid.Parse(request.DeliveryOptionId))
        );
    }
}
