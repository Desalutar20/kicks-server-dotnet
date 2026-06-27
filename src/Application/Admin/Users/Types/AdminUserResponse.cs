namespace Application.Admin.Users.Types;

public sealed record AdminUserResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public required string Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public Role Role { get; init; }
    public Gender? Gender { get; init; }
    public bool IsVerified { get; init; }
    public bool IsBanned { get; init; }
    public string? GoogleId { get; init; }
    public string? FacebookId { get; init; }
}

internal static class AdminUserResponseMapper
{
    public static AdminUserResponse ToAdminUser(this User user) =>
        new()
        {
            Id = user.Id.Value,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Email = user.Email.Value,
            FirstName = user.FirstName?.Value,
            LastName = user.LastName?.Value,
            Role = user.Role,
            Gender = user.Gender,
            IsVerified = user.IsVerified,
            IsBanned = user.IsBanned,
            GoogleId = user.GoogleId?.Value,
            FacebookId = user.FacebookId?.Value,
        };
}
