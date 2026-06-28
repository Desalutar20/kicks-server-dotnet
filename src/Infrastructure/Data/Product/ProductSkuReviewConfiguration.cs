using Domain.Products.ProductSkus.ProductSkuReviews;
using Domain.Shared.FileContent;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Product;

public class ProductSkuReviewConfiguration : IEntityTypeConfiguration<ProductSkuReview>
{
    public void Configure(EntityTypeBuilder<ProductSkuReview> builder)
    {
        var statuses = string.Join(
            ", ",
            Enum.GetNames<ProductSkuReviewStatus>().Select(r => $"'{r.ToLower()}'")
        );

        builder.ToTable(
            "product_sku_review",
            table =>
            {
                table.HasCheckConstraint("CK_status", $"status IN ({statuses})");
            }
        );

        builder.HasKey(x => x.Id);

        builder
            .HasIndex(x => new { x.ProductSkuId, x.UserId })
            .IsUnique()
            .HasDatabaseName(DbConstants.ProductSkuReviewUniqueUserPerSkuIndex);

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProductSkuReviewId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder
            .Property(x => x.Description)
            .HasConversion(c => c.Value, value => ProductSkuReviewDescription.Create(value).Value)
            .IsRequired();

        builder
            .Property(x => x.Rating)
            .HasConversion(c => c.Value, value => ProductSkuReviewRating.Create(value).Value)
            .IsRequired();

        builder
            .Property(x => x.Status)
            .HasConversion(
                status => status.ToString().ToLower(),
                value => Enum.Parse<ProductSkuReviewStatus>(value, true)
            )
            .IsRequired()
            .HasDefaultValue(ProductSkuReviewStatus.Pending);

        builder.OwnsMany(
            x => x.Images,
            x =>
            {
                x.ToJson("images");

                x.WithOwner().HasForeignKey("ProductSkuReviewId");

                x.Property(x => x.Id).IsRequired();

                x.Property(x => x.Url)
                    .HasConversion(x => x.Value.ToString(), value => FileUrl.Create(value).Value)
                    .IsRequired()
                    .HasMaxLength(FileUrl.MaxLength);

                x.Property(x => x.Name)
                    .HasConversion(x => x.FullName, value => FileName.Create(value).Value)
                    .IsRequired()
                    .HasMaxLength(FileName.MaxLength);
            }
        );

        builder
            .Property(x => x.ProductSkuId)
            .HasConversion(id => id.Value, value => new ProductSkuId(value))
            .IsRequired();

        builder
            .Property(x => x.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder
            .HasOne<ProductSku>()
            .WithMany()
            .HasForeignKey(x => x.ProductSkuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<DomainUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
