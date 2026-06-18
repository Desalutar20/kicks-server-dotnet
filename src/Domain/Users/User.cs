using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Users;

public sealed class User(
    Email email,
    HashedPassword? hashedPassword,
    FirstName? firstName,
    LastName? lastName,
    Gender? gender,
    ProviderId? googleId,
    ProviderId? facebookId
) : Entity<UserId>(new UserId(Guid.NewGuid()))
{
    public FirstName? FirstName { get; private set; } = firstName;
    public LastName? LastName { get; private set; } = lastName;
    public Email Email { get; private set; } = email;
    public HashedPassword? HashedPassword { get; private set; } = hashedPassword;
    public Gender? Gender { get; private set; } = gender;
    public Role Role { get; private set; } = Role.Regular;
    public ProviderId? GoogleId { get; private set; } = googleId;
    public ProviderId? FacebookId { get; private set; } = facebookId;
    public bool IsVerified { get; private set; }
    public bool IsBanned { get; private set; }

    public bool IsValid => !IsBanned && IsVerified;

    public void ConfirmAccount() => IsVerified = true;

    public void UpdatePassword(HashedPassword newPassword)
    {
        HashedPassword = newPassword;
    }

    public void ToggleIsBanned()
    {
        IsBanned = !IsBanned;
    }

    public Result LinkOAuthProvider(OAuthProvider provider, ProviderId providerId)
    {
        switch (provider)
        {
            case OAuthProvider.Google:
                GoogleId ??= providerId;
                return Result.Success();

            case OAuthProvider.Facebook:
                FacebookId ??= providerId;
                return Result.Success();

            default:
                return Error.Failure("Unsupported provider");
        }
    }
}
