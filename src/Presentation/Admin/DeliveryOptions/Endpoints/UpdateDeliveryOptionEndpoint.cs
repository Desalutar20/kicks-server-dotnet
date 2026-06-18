using Application.Admin.DeliveryOptions.UseCases.UpdateDeliveryOption;
using Application.Auth.Types;
using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.DeliveryOptions.Endpoints;

public sealed record UpdateDeliveryOptionRequest(
    string? Title,
    string? Description,
    decimal? Price
);

public sealed class UpdateDeliveryOptionRequestValidator
    : AbstractValidator<UpdateDeliveryOptionRequest>
{
    public UpdateDeliveryOptionRequestValidator()
    {
        RuleFor(x => x.Title).ValidateNullableValueObject(DeliveryOptionTitle.Create);
        RuleFor(x => x.Description).ValidateNullableValueObject(DeliveryOptionDescription.Create);
        RuleFor(x => x.Price).ValidateNullableValueObject(x => Money.FromDollars(x!.Value));
    }
}

internal static partial class DeliveryOptionsEndpoints
{
    private static IEndpointRouteBuilder UpdateDeliveryOptionV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPatch(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    UpdateDeliveryOptionRequest request,
                    Guid id,
                    ICommandHandler<UpdateDeliveryOptionCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.UpdateDeliveryOption");

                    var command = request.ToCommand(id);
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(new ApiResponse<string>("success"));
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
            .WithName("UpdateDeliveryOption")
            .WithSummary("Update an existing delivery option.")
            .WithDescription("Update an existing delivery option. Requires admin privileges.");

        return endpoint;
    }

    private static UpdateDeliveryOptionCommand ToCommand(
        this UpdateDeliveryOptionRequest request,
        Guid id
    )
    {
        var title = request.Title is not null
            ? DeliveryOptionTitle.Create(request.Title).Value
            : null;
        var description = request.Description is not null
            ? DeliveryOptionDescription.Create(request.Description).Value
            : null;
        var price = request.Price is not null ? Money.FromDollars(request.Price.Value).Value : null;

        return new UpdateDeliveryOptionCommand(new DeliveryOptionId(id), title, description, price);
    }
}
