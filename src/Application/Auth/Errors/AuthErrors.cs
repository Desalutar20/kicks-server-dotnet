using Application.Auth.Types;

namespace Application.Auth.Errors;

internal static class AuthErrors
{
    public static Result<UserWithSessionId> InvalidCredentials =>
        Result<UserWithSessionId>.Failure(Error.Failure("Invalid credentials"));

    public static Error InvalidOrExpiredToken => Error.Failure("Invalid or expired token");

    public static Result<Guid> InvalidOAuthState =>
        Result<Guid>.Failure(Error.Failure("Invalid or expired token"));

    public static Result<SessionUser> Unauthorized =>
        Result<SessionUser>.Failure(Error.Unauthorized("Unauthorized"));
}
