using Domain.Abstractions;
using Domain.Product.Brand;
using Domain.Product.Category;

namespace Domain.Product;

public sealed class Product : Entity<ProductId>
{
    private Product()
        : base(new ProductId(Guid.NewGuid())) { }

    public ProductTitle Title { get; private set; } = null!;
    public ProductDescription Description { get; private set; } = null!;
    public ProductGender Gender { get; private set; }
    public ProductTags Tags { get; private set; } = null!;
    public bool IsDeleted { get; private set; }

    public BrandId? BrandId { get; private set; }
    public CategoryId? CategoryId { get; private set; }

    public Brand.Brand? Brand { get; private set; }
    public Category.Category? Category { get; private set; }

    public static Product Create(
        ProductTitle title,
        ProductDescription description,
        ProductGender gender,
        ProductTags tags,
        BrandId brandId,
        CategoryId categoryId
    ) =>
        new()
        {
            Title = title,
            Description = description,
            Gender = gender,
            Tags = tags,
            BrandId = brandId,
            CategoryId = categoryId,
        };

    public void ToggleIsDeleted()
    {
        IsDeleted = !IsDeleted;
    }

    public void Update(
        ProductTitle? title,
        ProductDescription? description,
        ProductGender? gender,
        ProductTags? tags,
        BrandId? brandId,
        CategoryId? categoryId
    )
    {
        Title = title ?? Title;
        Description = description ?? Description;
        Gender = gender ?? Gender;
        Tags = tags ?? Tags;
        BrandId = brandId ?? BrandId;
        CategoryId = categoryId ?? CategoryId;
    }
}
