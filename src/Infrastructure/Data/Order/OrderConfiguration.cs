using Domain.DeliveryOptions;
using Domain.Orders;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Order;

public class CartConfiguration : IEntityTypeConfiguration<DomainOrder>
{
    public void Configure(EntityTypeBuilder<DomainOrder> builder)
    {
        var statuses = string.Join(
            ", ",
            Enum.GetNames<OrderStatus>().Select(g => $"'{g.ToLower()}'")
        );

        builder.Ignore(x => x.Total);
        builder.Ignore(x => x.IsExpired);

        builder.ToTable(
            "order",
            table =>
            {
                table.HasCheckConstraint("CK_order_status", $"status IN ({statuses})");
            }
        );

        builder
            .HasIndex(x => new { x.UserId, x.PromocodeId })
            .IsUnique()
            .HasFilter(
                """
                    "promocode_id" IS NOT NULL
                    AND "status" <> 'cancelled'
                """
            )
            .HasDatabaseName(DbConstants.OrderUserPromocodeUniqueIndex);

        builder
            .HasIndex(x => x.UserId)
            .IsUnique()
            .HasFilter(
                """
                status = 'pending'
                """
            )
            .HasDatabaseName(DbConstants.OrderUserPendingUniqueIndex);

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new OrderId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnType("timestamptz").IsRequired();

        builder
            .Property(x => x.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder
            .Property(x => x.PhoneNumber)
            .HasConversion(
                phoneNumber => phoneNumber.Value,
                value => PhoneNumber.Create(value).Value
            )
            .IsRequired();

        builder.ComplexProperty(
            x => x.BillingAddress,
            address =>
            {
                address.IsRequired(false);

                address.Property(x => x.City).HasColumnName("billing_address_city");
                address.Property(x => x.Street).HasColumnName("billing_address_street");
                address.Property(x => x.Home).HasColumnName("billing_address_home");
                address.Property(x => x.Apartment).HasColumnName("billing_address_apartment");
            }
        );

        builder.ComplexProperty(
            x => x.DeliveryAddress,
            address =>
            {
                address.Property(x => x.City).HasColumnName("delivery_address_city").IsRequired();
                address
                    .Property(x => x.Street)
                    .HasColumnName("delivery_address_street")
                    .IsRequired();
                address.Property(x => x.Home).HasColumnName("delivery_address_home").IsRequired();
                address
                    .Property(x => x.Apartment)
                    .HasColumnName("delivery_address_apartment")
                    .IsRequired();
            }
        );

        builder
            .Property(x => x.Status)
            .HasConversion(
                status => status.ToString().ToLower(),
                value => Enum.Parse<OrderStatus>(value, true)
            )
            .IsRequired()
            .HasDefaultValue(OrderStatus.Pending);

        builder
            .Property(x => x.DeliveryPrice)
            .HasConversion(v => v.Cents, v => Money.FromCents(v).Value)
            .IsRequired();

        builder.OwnsMany(
            x => x.OrderItems,
            b =>
            {
                b.WithOwner().HasForeignKey("OrderId");

                b.HasKey("Id");
                b.HasIndex("OrderId", "ProductSkuId")
                    .IsUnique()
                    .HasDatabaseName(DbConstants.OrderItemUniqueIndex);

                b.Property<Guid>("Id");

                b.Property(x => x.Quantity)
                    .HasConversion(v => v.Value, v => PositiveInt.Create(v).Value)
                    .IsRequired();

                b.Property(x => x.Price)
                    .HasConversion(v => v.Cents, v => Money.FromCents(v).Value)
                    .IsRequired();

                b.Property(x => x.ProductSkuId)
                    .HasConversion(v => v.Value, v => new ProductSkuId(v))
                    .IsRequired();

                b.HasOne(x => x.ProductSku)
                    .WithMany()
                    .HasForeignKey(x => x.ProductSkuId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.Navigation(x => x.ProductSku).AutoInclude();
            }
        );

        builder.OwnsMany(
            x => x.OrderPayments,
            b =>
            {
                var paymentStatuses = string.Join(
                    ", ",
                    Enum.GetNames<OrderPaymentStatus>().Select(g => $"'{g.ToLower()}'")
                );

                b.ToTable(
                    "order_payment",
                    table =>
                    {
                        table.HasCheckConstraint(
                            "CK_order_payment_status",
                            $"status IN ({paymentStatuses})"
                        );
                    }
                );

                b.WithOwner().HasForeignKey("OrderId");

                b.HasKey("Id");
                b.HasIndex(x => x.TransactionId).IsUnique();

                b.Property<Guid>("Id").IsRequired();

                b.Property(x => x.TransactionId)
                    .HasConversion(v => v.Value, v => OrderPaymentTransactionId.Create(v).Value)
                    .HasMaxLength(OrderPaymentTransactionId.MaxLength)
                    .IsRequired();

                b.Property(x => x.Amount)
                    .HasConversion(v => v.Cents, v => Money.FromCents(v).Value)
                    .IsRequired();

                b.Property(x => x.Status)
                    .HasConversion(
                        status => status.ToString().ToLower(),
                        value => Enum.Parse<OrderPaymentStatus>(value, true)
                    )
                    .IsRequired()
                    .HasDefaultValue(OrderPaymentStatus.Pending);
            }
        );

        builder
            .Property(x => x.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder
            .HasOne<DomainUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.DeliveryOptionId)
            .HasConversion(id => id.Value, value => new DeliveryOptionId(value))
            .IsRequired();

        builder
            .HasOne(x => x.DeliveryOption)
            .WithMany()
            .HasForeignKey(x => x.DeliveryOptionId)
            .OnDelete(DeleteBehavior.Restrict);

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

        builder.Navigation(x => x.OrderItems).AutoInclude();
        builder.Navigation(x => x.OrderPayments).AutoInclude();
        builder.Navigation(x => x.Promocode).AutoInclude();
        builder.Navigation(x => x.DeliveryOption).AutoInclude();
    }
}
