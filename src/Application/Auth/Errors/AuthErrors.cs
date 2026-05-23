using Application.Auth.Types;

namespace Application.Auth.Errors;

internal static class AuthErrors
{
    public static Result<UserWithSessionId> InvalidCredentials =>
        Error.Failure("Invalid credentials");

    public static Error InvalidOrExpiredToken => Error.Failure("Invalid or expired token");

    public static Result<Guid> InvalidOAuthState => Error.Failure("Invalid or expired token");

    public static Result<SessionUser> Unauthorized => Error.Unauthorized("Unauthorized");
}
