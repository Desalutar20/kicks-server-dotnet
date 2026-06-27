using Domain.Shared.FileContent;
using Domain.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Product;

public class ProductSkuConfiguration : IEntityTypeConfiguration<ProductSku>
{
    public void Configure(EntityTypeBuilder<ProductSku> builder)
    {
        builder.Ignore(x => x.RemainingImageSlots);

        builder.ToTable(
            "product_sku",
            table =>
            {
                table.HasCheckConstraint("CK_product_sku_quantity_positive", "quantity >= 0");
            }
        );

        builder.HasKey(x => x.Id);

        builder
            .HasIndex(x => x.Sku)
            .IsUnique()
            .HasDatabaseName(DbConstants.ProductSkuSkuUniqueIndex);
        builder
            .HasIndex(x => new
            {
                x.ProductId,
                x.Size,
                x.Color,
            })
            .IsUnique()
            .HasDatabaseName(DbConstants.ProductSkuDuplicateCombinationUniqueIndex);

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProductSkuId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.ComplexProperty(
            x => x.Price,
            price =>
            {
                price
                    .Property(p => p.Price)
                    .HasConversion(p => p.Cents, value => Money.FromCents(value).Value)
                    .HasColumnName("price")
                    .IsRequired();

                price
                    .Property(x => x.SalePrice)
                    .HasConversion<long?>(
                        p => p == null ? null : p.Cents,
                        value => value == null ? null : Money.FromCents(value.Value).Value
                    )
                    .HasColumnName("sale_price");
            }
        );

        builder.Property(x => x.Quantity).IsRequired();

        builder
            .Property(x => x.Size)
            .HasConversion(s => s.Value, value => PositiveInt.Create(value).Value)
            .IsRequired();

        builder
            .Property(x => x.Color)
            .HasConversion(c => c.Value, value => ProductSkuColor.Create(value).Value)
            .IsRequired();

        builder
            .Property(x => x.Sku)
            .HasConversion(c => c.Value, value => ProductSkuSku.Create(value).Value)
            .IsRequired();

        builder.OwnsMany(
            x => x.Images,
            x =>
            {
                x.ToJson("images");

                x.WithOwner().HasForeignKey("ProductSkuId");

                x.Property(x => x.Id).IsRequired().HasJsonPropertyName("id");

                x.Property(x => x.Url)
                    .HasConversion(q => q.Value.ToString(), value => FileUrl.Create(value).Value)
                    .IsRequired()
                    .HasMaxLength(FileUrl.MaxLength)
                    .HasJsonPropertyName("url");

                x.Property(x => x.Name)
                    .HasConversion(q => q.FullName, value => FileName.Create(value).Value)
                    .IsRequired()
                    .HasMaxLength(FileName.MaxLength)
                    .HasJsonPropertyName("name");
            }
        );

        builder
            .Property(x => x.ProductId)
            .HasConversion(id => id.Value, value => new ProductId(value))
            .IsRequired();

        builder
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Product).AutoInclude();
        builder.Navigation(x => x.Images).AutoInclude();
    }
}
