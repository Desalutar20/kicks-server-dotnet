using Domain.Product.ProductSku;
using Domain.Product.ProductSku.ProductSkuImage;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Product;

public class ProductSkuImageConfiguration : IEntityTypeConfiguration<ProductSkuImage>
{
    public void Configure(EntityTypeBuilder<ProductSkuImage> builder)
    {
        builder.ToTable("product_sku_image");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProductSkuImageId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder
            .Property(x => x.ImageUrl)
            .HasConversion(q => q.Value, value => ProductSkuImageUrl.Create(value).Value)
            .IsRequired();

        builder.Property(x => x.ImageId).IsRequired();

        builder
            .Property(x => x.ImageName)
            .HasConversion(q => q.Value, value => ProductSkuImageName.Create(value).Value)
            .IsRequired();

        builder
            .Property(x => x.ProductSkuId)
            .HasConversion(id => id.Value, value => new ProductSkuId(value))
            .IsRequired();

        builder
            .HasOne<ProductSku>()
            .WithMany(x => x.ProductSkuImages)
            .HasForeignKey(x => x.ProductSkuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
