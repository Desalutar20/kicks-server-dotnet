using Domain.Abstractions;
using Domain.Brands;
using Domain.Categories;

namespace Domain.Products;

public sealed class Product(
    ProductTitle title,
    ProductDescription description,
    ProductGender gender,
    ProductTags tags,
    BrandId brandId,
    CategoryId categoryId
) : Entity<ProductId>(new ProductId(Guid.NewGuid()))
{
    public ProductTitle Title { get; private set; } = title;
    public ProductDescription Description { get; private set; } = description;
    public ProductGender Gender { get; private set; } = gender;
    public ProductTags Tags { get; private set; } = tags;
    public bool IsDeleted { get; private set; }

    public BrandId? BrandId { get; private set; } = brandId;
    public CategoryId? CategoryId { get; private set; } = categoryId;

    public Brand? Brand { get; private set; } = null!;
    public Category? Category { get; private set; } = null!;

    public void ToggleIsDeleted()
    {
        IsDeleted = !IsDeleted;
    }

    public void Update(
        ProductTitle title,
        ProductDescription description,
        ProductGender gender,
        ProductTags tags,
        BrandId? brandId,
        CategoryId? categoryId
    )
    {
        Title = title;
        Description = description;
        Gender = gender;
        Tags = tags;
        BrandId = brandId;
        CategoryId = categoryId;
    }
}
