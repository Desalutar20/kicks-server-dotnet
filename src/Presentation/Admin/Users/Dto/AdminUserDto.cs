using Application.Admin.Users.Types;

namespace Presentation.Admin.Users.Dto;

public sealed record AdminUserDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Email,
    string? FirstName,
    string? LastName,
    Role Role,
    Gender? Gender,
    bool IsVerified,
    bool IsBanned,
    ProviderId? GoogleId,
    ProviderId? FacebookId
);

internal static class AdminUserDtoMapper
{
    public static AdminUserDto ToDto(this AdminUser user) =>
        new(
            user.Id.Value,
            user.CreatedAt,
            user.UpdatedAt,
            user.Email.ToString(),
            user.FirstName?.Value,
            user.LastName?.Value,
            user.Role,
            user.Gender,
            user.IsVerified,
            user.IsBanned,
            user.GoogleId,
            user.FacebookId
        );
}
