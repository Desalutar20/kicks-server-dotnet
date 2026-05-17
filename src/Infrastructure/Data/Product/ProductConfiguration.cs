using Domain.Brand;
using Domain.Category;
using Domain.Product;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Product;

public class ProductConfiguration : IEntityTypeConfiguration<DomainProduct>
{
    public void Configure(EntityTypeBuilder<DomainProduct> builder)
    {
        var genders = string.Join(
            ", ",
            Enum.GetNames<ProductGender>().Select(r => $"'{r.ToLower()}'")
        );

        builder.ToTable(
            "product",
            table =>
            {
                table.HasCheckConstraint("CK_gender", $"gender IN ({genders})");
            }
        );

        builder.HasKey(x => x.Id);

        builder
            .HasIndex(x => new
            {
                x.Title,
                x.Gender,
                x.CategoryId,
                x.BrandId,
            })
            .IsUnique()
            .HasDatabaseName(DbConstants.ProductUniqueIndex);

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProductId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder
            .Property(x => x.Title)
            .HasConversion(name => name.Value, value => ProductTitle.Create(value).Value)
            .HasMaxLength(ProductTitle.MaxLength)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasConversion(name => name.Value, value => ProductDescription.Create(value).Value)
            .HasMaxLength(ProductDescription.MaxLength)
            .IsRequired();

        builder
            .Property(x => x.Gender)
            .HasConversion(
                role => role.ToString().ToLower(),
                value => Enum.Parse<ProductGender>(value, true)
            )
            .IsRequired();

        builder
            .Property(x => x.Tags)
            .HasConversion(tags => tags.Value, value => ProductTags.Create(value).Value)
            .IsRequired()
            .HasDefaultValueSql("'{}'::text[]")
            .Metadata.SetValueComparer(
                new ValueComparer<ProductTags>(
                    (a, b) => a!.Value.SequenceEqual(b!.Value),
                    tags =>
                        tags.Value.Aggregate(
                            0,
                            (hash, item) => HashCode.Combine(hash, item.GetHashCode())
                        ),
                    tags => ProductTags.Create(tags.Value.ToList()).Value
                )
            );

        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder
            .Property(x => x.BrandId)
            .HasConversion<Guid?>(
                id => id != null ? id.Value : null,
                value => value != null ? new BrandId(value.Value) : null
            )
            .IsRequired(false);

        builder
            .Property(x => x.CategoryId)
            .HasConversion<Guid?>(
                id => id != null ? id.Value : null,
                value => value != null ? new CategoryId(value.Value) : null
            )
            .IsRequired(false);

        builder
            .HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
