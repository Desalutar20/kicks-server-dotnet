using Application.Admin.DeliveryOptions.UseCases.GetAdminDeliveryOptions;
using Application.Auth.Types;
using Domain.DeliveryOptions;
using Presentation.DeliveryOptions.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.DeliveryOptions.Endpoints;

internal static partial class DeliveryOptionsEndpoints
{
    private static IEndpointRouteBuilder GetDeliveryOptionsV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/",
                async (
                    HttpContext ctx,
                    IQueryHandler<
                        GetAdminDeliveryOptionsQuery,
                        IReadOnlyList<DeliveryOption>
                    > queryHandler,
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

                    var logger = loggerFactory.CreateLogger("GetDeliveryOptions");

                    var result = await queryHandler.Handle(new GetAdminDeliveryOptionsQuery(), ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Ok(
                        new ApiResponse<IReadOnlyList<DeliveryOptionDto>>(
                            result.Value.Select(x => x.ToDto()).ToList()
                        )
                    );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .Produces<ApiResponse<IReadOnlyList<DeliveryOptionDto>>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetDeliveryOptions")
            .WithSummary("Retrieves list of delivery options.")
            .WithDescription("Returns a list of delivery options. ");

        return endpoint;
    }
}
