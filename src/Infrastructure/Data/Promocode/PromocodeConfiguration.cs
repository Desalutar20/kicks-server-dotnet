using Domain.Promocodes;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Promocode;

public class ProductConfiguration : IEntityTypeConfiguration<DomainPromocode>
{
    public void Configure(EntityTypeBuilder<DomainPromocode> builder)
    {
        var types = string.Join(
            ", ",
            Enum.GetNames<PromocodeType>().Select(r => $"'{r.ToLower()}'")
        );

        builder.Ignore(x => x.IsActive);
        builder.Ignore(x => x.IsExpired);
        builder.Ignore(x => x.HasUsagesLeft);

        builder.ToTable(
            "promocode",
            table =>
            {
                table.HasCheckConstraint("CK_type", $"type IN ({types})");
                table.HasCheckConstraint("CK_usage_count", "usage_count < usage_limit");
                table.HasCheckConstraint(
                    "CK_discount_value_percent",
                    $"type != '{nameof(PromocodeType.Percent).ToLower()}' OR (discount_value > 0 AND discount_value < 100)"
                );
                table.HasCheckConstraint("CK_validity_period", "valid_from < valid_to");
            }
        );

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName(DbConstants.PromocodeUniqueIndex);

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PromocodeId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder
            .Property(x => x.DiscountValue)
            .HasConversion(
                discountValue => discountValue.Value,
                value => PositiveInt.Create(value).Value
            )
            .IsRequired();

        builder
            .Property(x => x.Type)
            .HasConversion(
                type => type.ToString().ToLower(),
                value => Enum.Parse<PromocodeType>(value, true)
            )
            .IsRequired();

        builder.ComplexProperty(
            x => x.ValidityPeriod,
            date =>
            {
                date.Property(x => x.ValidFrom).HasColumnName("valid_from").IsRequired();
                date.Property(x => x.ValidTo).HasColumnName("valid_to").IsRequired();
            }
        );

        builder
            .Property(x => x.UsageLimit)
            .HasConversion(
                discountValue => discountValue.Value,
                value => PositiveInt.Create(value).Value
            )
            .IsRequired();

        builder.Property(x => x.UsageCount).IsRequired().HasDefaultValue(0);

        builder
            .Property(x => x.Code)
            .HasConversion(code => code.Value, value => PromocodeCode.Create(value).Value)
            .IsRequired();
    }
}
