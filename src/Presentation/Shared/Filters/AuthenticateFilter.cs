using Application.Auth.Types;
using Application.Auth.UseCases.Authenticate;
using Application.Config;

namespace Presentation.Shared.Filters;

public class AuthenticateFilter(
    ICommandHandler<AuthenticateCommand, SessionUser> commandHandler,
    Config config
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var sessionCookieName = config.Application.SessionCookieName;

        if (
            !context.HttpContext.Request.Cookies.TryGetValue(sessionCookieName, out var sessionId)
            || !Guid.TryParse(sessionId, out var guid)
        )
        {
            return TypedResults.Problem(
                "Unauthorized",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        var command = new AuthenticateCommand(guid);
        var result = await commandHandler.Handle(command, context.HttpContext.RequestAborted);
        if (result.IsFailure)
        {
            CookieOptions cookieOptions = new() { Expires = DateTimeOffset.UtcNow.AddDays(-1) };
            context.HttpContext.Response.Cookies.Append(sessionCookieName, "", cookieOptions);

            return TypedResults.Problem(
                "Unauthorized",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        context.HttpContext.Items[RequestConstants.UserKey] = result.Value;
        context.HttpContext.Items[RequestConstants.SessionKey] = guid;

        return await next(context);
    }
}
