using Domain.Abstractions;

namespace Domain.Brand;

public sealed class Brand : Entity<BrandId>
{
    private Brand()
        : base(new BrandId(Guid.NewGuid())) { }

    public BrandName Name { get; private set; } = null!;

    public static Brand Create(BrandName name) => new() { Name = name };

    public void UpdateName(BrandName name) => Name = name;
}
