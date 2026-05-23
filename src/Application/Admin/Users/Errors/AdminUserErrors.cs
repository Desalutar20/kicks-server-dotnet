namespace Application.Admin.Users.Errors;

internal static class AdminUserErrors
{
    public static Result UserNotFound(UserId userId) =>
        Error.Failure($"User with id '{userId}' doesn't exist");
}
