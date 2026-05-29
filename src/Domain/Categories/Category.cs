using Domain.Abstractions;

namespace Domain.Categories;

public sealed class Category(CategoryName name) : Entity<CategoryId>(new CategoryId(Guid.NewGuid()))
{
    public CategoryName Name { get; private set; } = name;

    public void UpdateName(CategoryName name) => Name = name;
}
