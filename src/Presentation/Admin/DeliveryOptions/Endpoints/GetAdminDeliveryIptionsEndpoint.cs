using Application.Admin.DeliveryOptions.UseCases.GetAdminDeliveryOptions;
using Application.Auth.Types;
using Domain.DeliveryOptions;
using Presentation.Admin.DeliveryOptions.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.DeliveryOptions.Endpoints;

internal static partial class AdminDeliveryOptionsEndpoints
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

                    var logger = loggerFactory.CreateLogger("Admin.GetDeliveryOptions");

                    var result = await queryHandler.Handle(new GetAdminDeliveryOptionsQuery(), ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Ok(
                        new ApiResponse<IReadOnlyList<AdminDeliveryOptionDto>>(
                            result.Value.Select(x => x.ToDto()).ToList()
                        )
                    );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .Produces<ApiResponse<IReadOnlyList<AdminDeliveryOptionDto>>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetAdminDeliveryOptions")
            .WithSummary("Retrieves list of delivery options for admin panel.")
            .WithDescription("Returns a list of delivery options. ");

        return endpoint;
    }
}
