using Application.Admin.DeliveryOptions.UseCases.CreateDeliveryOption;
using Application.Auth.Types;
using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;
using Presentation.Admin.DeliveryOptions.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.DeliveryOptions.Endpoints;

public sealed record CreateDeliveryOptionRequest(string Title, string Description, decimal Price);

public sealed class CreateDeliveryOptionRequestValidator
    : AbstractValidator<CreateDeliveryOptionRequest>
{
    public CreateDeliveryOptionRequestValidator()
    {
        RuleFor(x => x.Title).ValidateValueObject(DeliveryOptionTitle.Create);
        RuleFor(x => x.Description).ValidateValueObject(DeliveryOptionDescription.Create);
        RuleFor(x => x.Price).ValidateValueObject(Money.FromDollars);
    }
}

internal static partial class DeliveryOptionsEndpoints
{
    private static IEndpointRouteBuilder CreateDeliveryOptionV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/",
                async (
                    HttpContext ctx,
                    CreateDeliveryOptionRequest request,
                    ICommandHandler<CreateDeliveryOptionCommand, DeliveryOption> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.CreateDeliveryOption");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);
                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Created(
                            "/",
                            new ApiResponse<AdminDeliveryOptionDto>(result.Value.ToAdminDto())
                        );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<AdminDeliveryOptionDto>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("CreateDeliveryOption")
            .WithSummary("Creates a new delivery option.")
            .WithDescription("Creates a new delivery option. Requires admin privileges.");

        return endpoint;
    }

    private static CreateDeliveryOptionCommand ToCommand(this CreateDeliveryOptionRequest request)
    {
        var title = DeliveryOptionTitle.Create(request.Title).Value;
        var description = DeliveryOptionDescription.Create(request.Description).Value;
        var price = Money.FromDollars(request.Price).Value;

        return new CreateDeliveryOptionCommand(title, description, price);
    }
}
