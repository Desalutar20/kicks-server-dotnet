using Domain.Product.Category;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Product;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("category");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new CategoryId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder
            .Property(x => x.Name)
            .HasConversion(name => name.Value, value => CategoryName.Create(value).Value)
            .HasMaxLength(CategoryName.MaxLength)
            .IsRequired();
    }
}
