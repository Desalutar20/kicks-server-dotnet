using Domain.Abstractions;

namespace Domain.Brands;

public sealed class Brand(BrandName name) : Entity<BrandId>(new BrandId(Guid.NewGuid()))
{
    public BrandName Name { get; private set; } = name;

    public void UpdateName(BrandName name) => Name = name;
}
