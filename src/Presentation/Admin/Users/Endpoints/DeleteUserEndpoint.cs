using Application.Admin.Users.UseCases.DeleteUser;
using Application.Auth.Types;
using Presentation.Shared;

namespace Presentation.Admin.Users.Endpoints;

internal static partial class AdminUsersEndpoints
{
    private static IEndpointRouteBuilder DeleteUserV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapDelete(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    ICommandHandler<DeleteUserCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.DeleteUser");

                    var command = new DeleteUserCommand(new UserId(id));
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(new ApiResponse<string>("Success"));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("DeleteUser")
            .WithSummary("Delete user")
            .WithDescription(
                "Permanently deletes a user by their unique identifier. This action cannot be undone."
            );

        return endpoint;
    }
}
