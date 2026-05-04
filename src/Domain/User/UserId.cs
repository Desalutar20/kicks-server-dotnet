namespace Domain.User;

public sealed record UserId(Guid Value)
{
    public static implicit operator Guid(UserId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
