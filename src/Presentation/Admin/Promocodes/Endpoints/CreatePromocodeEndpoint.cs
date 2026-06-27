using Application.Admin.Promocodes.Types;
using Application.Admin.Promocodes.UseCases.CreatePromocode;
using Application.Auth.Types;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;
using Presentation.Admin.Promocodes.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.Promocodes.Endpoints;

public sealed record CreatePromocodeRequest(
    int DiscountValue,
    string Type,
    PromocodeValidityPeriodDto ValidityPeriod,
    int UsageLimit,
    string Code
);

public sealed class CreatePromocodeRequestValidator : AbstractValidator<CreatePromocodeRequest>
{
    public CreatePromocodeRequestValidator()
    {
        RuleFor(x => x.DiscountValue)
            .ValidateValueObject(x => PositiveInt.Create(x, label: "Discount value"));

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(g => Enum.TryParse<PromocodeType>(g, true, out _))
            .WithMessage(
                $"Type must be one of: {string.Join(", ", Enum.GetNames<PromocodeType>())}"
            );

        RuleFor(x => x.ValidityPeriod)
            .ValidateValueObject(x => PromocodeValidityPeriod.Create(x.ValidFrom, x.ValidTo));

        RuleFor(x => x.UsageLimit)
            .ValidateValueObject(x => PositiveInt.Create(x, label: "Usage limit"));

        RuleFor(x => x.Code).ValidateValueObject(PromocodeCode.Create);

        RuleFor(x => x)
            .Must(x =>
                !Enum.TryParse<PromocodeType>(x.Type, true, out var type)
                || type != PromocodeType.Percent
                || (x.DiscountValue > 0 && x.DiscountValue < 100)
            )
            .WithMessage("Percent discount must be greater than 0 and less than 100.")
            .WithName("discountValue");
    }
}

internal static partial class AdminPromocodesEndpoints
{
    private static IEndpointRouteBuilder CreatePromocodeV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/",
                async (
                    HttpContext ctx,
                    CreatePromocodeRequest request,
                    ICommandHandler<CreatePromocodeCommand, AdminPromocodeResponse> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.CreatePromocode");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Created(
                            "/",
                            new ApiResponse<AdminPromocodeResponse>(result.Value)
                        );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<AdminPromocodeResponse>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("CreatePromocode")
            .WithSummary("Creates a new promocode.")
            .WithDescription("Creates a new promocode. Requires admin privileges.");

        return endpoint;
    }

    private static CreatePromocodeCommand ToCommand(this CreatePromocodeRequest request)
    {
        var discountValue = PositiveInt.Create(request.DiscountValue).Value;
        var type = Enum.Parse<PromocodeType>(request.Type, true);
        var validityPeriod = PromocodeValidityPeriod
            .Create(request.ValidityPeriod.ValidFrom, request.ValidityPeriod.ValidTo)
            .Value;
        var usageLimit = PositiveInt.Create(request.UsageLimit).Value;
        var code = PromocodeCode.Create(request.Code).Value;

        return new CreatePromocodeCommand(discountValue, type, validityPeriod, usageLimit, code);
    }
}
