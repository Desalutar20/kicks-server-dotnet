namespace Application.Auth.Errors;

internal static class AuthErrors
{
    public static Error InvalidCredentials => Error.Failure("Invalid credentials");

    public static Error InvalidOrExpiredToken => Error.Failure("Invalid or expired token");

    public static Error InvalidOAuthState => Error.Failure("Invalid or expired token");

    public static Error Unauthorized => Error.Unauthorized("Unauthorized");
}
