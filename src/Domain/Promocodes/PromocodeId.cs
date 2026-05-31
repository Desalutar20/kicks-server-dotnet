namespace Domain.Promocodes;

public sealed record PromocodeId(Guid Value)
{
    public static implicit operator Guid(PromocodeId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
