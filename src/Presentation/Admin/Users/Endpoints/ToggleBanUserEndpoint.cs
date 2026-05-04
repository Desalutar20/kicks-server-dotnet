using Application.Admin.Users.UseCases.ToggleBanUser;
using Application.Auth.Types;

namespace Presentation.Admin.Users.Endpoints;

internal static partial class AdminUsersEndpoints
{
    private static IEndpointRouteBuilder ToggleBanUserV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    ICommandHandler<ToggleBanUserCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.ToggleBanUser");

                    var command = new ToggleBanUserCommand(new UserId(id));
                    var result = await commandHandler.Handle(command, ct);
                    if (result.IsFailure)
                    {
                        return ErrorHandler.Handle(result.Error, logger);
                    }

                    return Results.Ok(new ApiResponse<string>("Success"));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("ToggleBanUser")
            .WithSummary("Toggle user ban status")
            .WithDescription(
                "Enables or disables a user's banned state based on their current status. "
            );

        return endpoint;
    }
}
