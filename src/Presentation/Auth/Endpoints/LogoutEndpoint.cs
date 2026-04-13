using Application.Auth.Types;
using Application.Auth.UseCases.Logout;
using Application.Config;

namespace Presentation.Auth.Endpoints;

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder LogoutV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/logout",
                    async (
                        HttpContext ctx, ICommandHandler<LogoutCommand> commandHandler, Config config,
                        LoggerFactory loggerFactory) =>
                    {
                        var logger = loggerFactory.CreateLogger("Auth.Logout");

                        if (!ctx.Items.TryGetValue(RequestConstants.UserKey, out var userResult) ||
                            !ctx.Items.TryGetValue(RequestConstants.SessionKey, out var sessionResult) ||
                            userResult is not SessionUser sessionUser ||
                            sessionResult is not Guid sessionId)
                        {
                            return Results.Unauthorized();
                        }

                        var command = new LogoutCommand(sessionUser.Id, sessionId);
                        var result = await commandHandler.Handle(command);
                        if (result.IsFailure)
                        {
                            return ErrorHandler.Handle(result.Error, logger);
                        }

                        ClearSessionCookie(ctx, config.Application);

                        return Results.Ok(new ApiResponse<string>("success"));
                    })
                .AddEndpointFilter<AuthenticateFilter>()
                .Produces<ApiResponse<string>>()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .WithName("Logout")
                .WithSummary("Logout current user")
                .WithDescription("Ends the current session and clears the authentication cookie.")
                .RequireRateLimiting(RateLimitConstants.Logout);

        return endpoint;
    }
}