using Domain.Carts;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Cart;

public class CartConfiguration : IEntityTypeConfiguration<DomainCart>
{
    public void Configure(EntityTypeBuilder<DomainCart> builder)
    {
        builder.ToTable("cart");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId).IsUnique().HasDatabaseName(DbConstants.CartUniqueIndex);

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new CartId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.OwnsMany(
            x => x.CartItems,
            b =>
            {
                b.WithOwner().HasForeignKey("CartId");

                b.HasKey("Id");
                b.HasIndex("CartId", "ProductSkuId")
                    .IsUnique()
                    .HasDatabaseName(DbConstants.CartItemUniqueIndex);

                b.Property<Guid>("Id").IsRequired();

                b.Property(x => x.ProductSkuId)
                    .HasConversion(v => v.Value, v => new ProductSkuId(v))
                    .IsRequired();

                b.Property(x => x.Quantity)
                    .HasConversion(v => v.Value, v => PositiveInt.Create(v).Value)
                    .IsRequired();

                b.Navigation(x => x.ProductSku).AutoInclude();
            }
        );

        builder
            .Property(x => x.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder
            .HasOne<DomainUser>()
            .WithOne()
            .HasForeignKey<DomainCart>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.PromocodeId)
            .HasConversion<Guid?>(
                id => id == null ? null : id.Value,
                value => value == null ? null : new PromocodeId(value.Value)
            )
            .IsRequired(false);

        builder
            .HasOne(x => x.Promocode)
            .WithMany()
            .HasForeignKey(x => x.PromocodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(x => x.CartItems).AutoInclude();
        builder.Navigation(x => x.Promocode).AutoInclude();
    }
}
