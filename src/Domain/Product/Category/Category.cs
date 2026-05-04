using Domain.Abstractions;

namespace Domain.Product.Category;

public sealed class Category : Entity<CategoryId>
{
    private Category()
        : base(new CategoryId(Guid.NewGuid())) { }

    public CategoryName Name { get; private set; } = null!;

    public static Category Create(CategoryName name) => new() { Name = name };

    public void UpdateName(CategoryName name) => Name = name;
}
