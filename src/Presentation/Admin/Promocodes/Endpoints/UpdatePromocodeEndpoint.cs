using Application.Admin.Promocodes.UseCases.UpdatePromocode;
using Application.Auth.Types;
using Domain.Promocodes;
using Presentation.Admin.Promocodes.Dto;

namespace Presentation.Admin.Promocodes.Endpoints;

public sealed record UpdatePromocodeRequest(
    int? DiscountValue,
    string? Type,
    PromocodeValidityPeriodDto? ValidityPeriod,
    int? UsageLimit,
    string? Code
);

public sealed class UpdatePromocodeRequestValidator : AbstractValidator<UpdatePromocodeRequest>
{
    public UpdatePromocodeRequestValidator()
    {
        RuleFor(x => x.DiscountValue)
            .ValidateNullableValueObject(x =>
                PositiveInt.Create(x!.Value, label: "Discount value")
            );

        RuleFor(x => x.Type)
            .Must(x => x is null || Enum.TryParse<PromocodeType>(x, true, out _))
            .WithMessage(
                $"Type must be one of: {string.Join(", ", Enum.GetNames<PromocodeType>())}"
            );

        RuleFor(x => x.ValidityPeriod)
            .ValidateNullableValueObject(x =>
                PromocodeValidityPeriod.Create(x.ValidFrom, x.ValidTo)
            );

        RuleFor(x => x.UsageLimit)
            .ValidateNullableValueObject(x => PositiveInt.Create(x!.Value, label: "Usage limit"));

        RuleFor(x => x.Code).ValidateNullableValueObject(PromocodeCode.Create);

        RuleFor(x => x)
            .Must(x =>
                x.DiscountValue is null
                || !Enum.TryParse<PromocodeType>(x.Type, true, out var type)
                || type != PromocodeType.Percent
                || (x.DiscountValue.Value > 0 && x.DiscountValue.Value < 100)
            )
            .WithMessage("Percent discount must be greater than 0 and less than 100.")
            .WithName("discountValue");
    }
}

internal static partial class AdminPromocodesEndpoints
{
    private static IEndpointRouteBuilder UpdatePromocodeV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPatch(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    UpdatePromocodeRequest request,
                    ICommandHandler<UpdatePromocodeCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.UpdatePromocode");

                    var command = request.ToCommand(id);
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(new ApiResponse<string>("Success"));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("UpdatePromocode")
            .WithSummary("Updates a promocode.")
            .WithDescription(
                "Updates a promocode in the admin panel. Requires admin privileges and validates the provided product data before creation."
            );

        return endpoint;
    }

    private static UpdatePromocodeCommand ToCommand(this UpdatePromocodeRequest request, Guid id)
    {
        var discountValue = request.DiscountValue is not null
            ? PositiveInt.Create(request.DiscountValue.Value).Value
            : null;
        PromocodeType? type = request.Type is not null
            ? Enum.Parse<PromocodeType>(request.Type, true)
            : null;
        var validityPeriod = request.ValidityPeriod is not null
            ? PromocodeValidityPeriod
                .Create(request.ValidityPeriod.ValidFrom, request.ValidityPeriod.ValidTo)
                .Value
            : null;

        var usageLimit = request.UsageLimit is not null
            ? PositiveInt.Create(request.UsageLimit.Value).Value
            : null;
        var code = request.Code is not null ? PromocodeCode.Create(request.Code).Value : null;

        return new UpdatePromocodeCommand(
            new PromocodeId(id),
            discountValue,
            type,
            validityPeriod,
            usageLimit,
            code
        );
    }
}
