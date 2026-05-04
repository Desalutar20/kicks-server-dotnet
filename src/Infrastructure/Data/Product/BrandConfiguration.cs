using Domain.Product.Brand;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Product;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("brand");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new BrandId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder
            .Property(x => x.Name)
            .HasConversion(name => name.Value, value => BrandName.Create(value).Value)
            .HasMaxLength(BrandName.MaxLength)
            .IsRequired();
    }
}
