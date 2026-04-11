using Application.Auth.Types;

namespace Presentation.Auth.Dto;

public sealed record UserDto(string Email, string? FirstName, string? LastName, Role Role, Gender? Gender);

internal static class UserDtoMapper
{
    public static UserDto ToDto(this SessionUser user) =>
        new(user.Email.ToString(), user.FirstName?.Value, user.LastName?.Value,
            user.Role, user.Gender);
}