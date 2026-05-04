namespace Application.Admin.Users.Types;

public sealed record AdminUser(
    UserId Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Email Email,
    FirstName? FirstName,
    LastName? LastName,
    Role Role,
    Gender? Gender,
    bool IsVerified,
    bool IsBanned,
    ProviderId? GoogleId,
    ProviderId? FacebookId
);

internal static class AdminUserMapper
{
    public static AdminUser ToAdminUser(this User user) =>
        new(
            user.Id,
            user.CreatedAt,
            user.UpdatedAt,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.Gender,
            user.IsVerified,
            user.IsBanned,
            user.GoogleId,
            user.FacebookId
        );
}
