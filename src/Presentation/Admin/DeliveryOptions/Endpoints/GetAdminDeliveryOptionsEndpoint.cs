using Application.Admin.DeliveryOptions.Types;
using Application.Admin.DeliveryOptions.UseCases.GetAdminDeliveryOptions;
using Application.Auth.Types;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.DeliveryOptions.Endpoints;

internal static partial class DeliveryOptionsEndpoints
{
    private static IEndpointRouteBuilder GetAdminDeliveryOptionsV1(
        this IEndpointRouteBuilder endpoint
    )
    {
        endpoint
            .MapGet(
                "/",
                async (
                    HttpContext ctx,
                    IQueryHandler<
                        GetAdminDeliveryOptionsQuery,
                        IReadOnlyList<AdminDeliveryOptionResponse>
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
                        new ApiResponse<IReadOnlyList<AdminDeliveryOptionResponse>>(result.Value)
                    );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .Produces<ApiResponse<IReadOnlyList<AdminDeliveryOptionResponse>>>()
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
