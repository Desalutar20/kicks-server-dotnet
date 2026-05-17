namespace Domain.Brand;

public sealed record BrandId(Guid Value)
{
    public static implicit operator Guid(BrandId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
