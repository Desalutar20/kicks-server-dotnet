namespace Application.Auth.Types;

public sealed record SessionUser(
    UserId Id,
    Email Email,
    FirstName? FirstName,
    LastName? LastName,
    Role Role,
    Gender? Gender);

internal static class SessionUserMapper
{
    public static SessionUser ToSessionUser(this User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Role, user.Gender);
}