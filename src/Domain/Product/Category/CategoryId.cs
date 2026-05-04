namespace Domain.Product.Category;

public sealed record CategoryId(Guid Value)
{
    public static implicit operator Guid(CategoryId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
