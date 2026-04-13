using Domain.Abstractions;

namespace Domain.User;

public class User : Entity<UserId>
{
    private User() : base(new UserId(Guid.NewGuid()))
    {
    }

    public FirstName? FirstName { get; private set; }
    public LastName? LastName { get; private set; }
    public Email Email { get; private set; }
    public HashedPassword? HashedPassword { get; private set; }
    public Gender? Gender { get; private set; }
    public Role Role { get; private set; } = Role.Regular;
    public ProviderId? GoogleId { get; private set; }
    public ProviderId? FacebookId { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsBanned { get; } = false;


    public static User Create(Email email, HashedPassword? hashedPassword, FirstName? firstName, LastName? lastName,
        Gender? gender, ProviderId? googleId, ProviderId? facebookId)
    {
        var user = new User
        {
            Email = email,
            HashedPassword = hashedPassword,
            FirstName = firstName,
            LastName = lastName,
            Gender = gender,
            GoogleId = googleId,
            FacebookId = facebookId
        };

        return user;
    }


    public bool IsValid() => !IsBanned && IsVerified;
    public void ConfirmAccount() => IsVerified = true;

    public void UpdatePassword(HashedPassword newPassword)
    {
        HashedPassword = newPassword;
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
                return Result.Failure(Error.Failure("Unsupported provider"));
        }
    }
}