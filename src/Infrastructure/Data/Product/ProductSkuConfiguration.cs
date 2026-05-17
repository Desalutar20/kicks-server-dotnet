using Domain.Product;
using Domain.Product.ProductSku;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Product;

public class ProductSkuConfiguration : IEntityTypeConfiguration<ProductSku>
{
    public void Configure(EntityTypeBuilder<ProductSku> builder)
    {
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

        builder.OwnsOne(
            x => x.Price,
            price =>
            {
                price
                    .Property(x => x.Price)
                    .HasConversion(p => p.Value, value => PositiveInt.Create(value).Value)
                    .HasColumnName("price")
                    .IsRequired();
                price
                    .Property(x => x.SalePrice)
                    .HasConversion<int?>(
                        p => p == null ? null : p.Value.Value,
                        value => value == null ? null : PositiveInt.Create(value.Value).Value
                    )
                    .HasColumnName("sale_price");
            }
        );

        builder
            .Property(x => x.Quantity)
            .HasConversion(q => q.Value, value => PositiveInt.Create(value).Value)
            .IsRequired();

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

        builder
            .Property(x => x.ProductId)
            .HasConversion(id => id.Value, value => new ProductId(value))
            .IsRequired();

        builder
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
