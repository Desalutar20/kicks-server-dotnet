using Application.Auth.Types;
using Application.Carts.UseCases.ApplyPromocode;
using Domain.Promocodes;
using Presentation.Shared.Extensions;

namespace Presentation.Cart.Endpoints;

public sealed record ApplyPromocodeRequest(string Code);

public sealed class ApplyPromocodeRequestValidator : AbstractValidator<ApplyPromocodeRequest>
{
    public ApplyPromocodeRequestValidator()
    {
        RuleFor(x => x.Code).ValidateValueObject(PromocodeCode.Create);
    }
}

internal static partial class OrderEndpoints
{
    private static IEndpointRouteBuilder ApplyPromocodeV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/promocode",
                async (
                    ApplyPromocodeRequest request,
                    ICommandHandler<ApplyPromocodeCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("ApplyPromocode");

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
            .WithName("ApplyPromocode")
            .WithSummary("Apply promocode")
            .WithDescription("Applies a promocode to the authenticated user's shopping cart.")
            .RequireRateLimiting(RateLimitConstants.ApplyPromocode);

        return endpoint;
    }

    private static ApplyPromocodeCommand ToCommand(
        this ApplyPromocodeRequest request,
        UserId userId
    )
    {
        var code = PromocodeCode.Create(request.Code).Value;

        return new ApplyPromocodeCommand(userId, code);
    }
}
