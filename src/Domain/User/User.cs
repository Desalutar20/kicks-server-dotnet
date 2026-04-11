using Domain.Abstractions;

namespace Domain.User;

public class User : Aggregate<UserId>
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
    public GoogleId? GoogleId { get; private set; }
    public FacebookId? FacebookId { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsBanned { get; } = false;


    public static User Create(Email email, HashedPassword? hashedPassword, FirstName? firstName, LastName? lastName,
        Gender? gender, GoogleId? googleId, FacebookId? facebookId)
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
}