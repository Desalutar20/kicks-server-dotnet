using Application.Auth.Types;

namespace Presentation.Shared.Filters;

public sealed class AuthorizeFilter(params Role[] roles) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        if (!httpContext.Items.TryGetValue(RequestConstants.UserKey, out var result) ||
            result is not SessionUser user)
            return TypedResults.Problem(
                "Unauthorized",
                statusCode: StatusCodes.Status401Unauthorized
            );

        if (roles.Length > 0 && !roles.Contains(user.Role))
            return TypedResults.Problem(
                "Access denied",
                statusCode: StatusCodes.Status403Forbidden
            );

        return await next(context);
    }
}