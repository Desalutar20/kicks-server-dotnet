using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.DeliveryOption;

public class DeliveryOptionConfiguration : IEntityTypeConfiguration<DomainDeliveryOption>
{
    public void Configure(EntityTypeBuilder<DomainDeliveryOption> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DeliveryOptionId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder
            .Property(x => x.Title)
            .HasConversion(title => title.Value, value => DeliveryOptionTitle.Create(value).Value)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasConversion(
                description => description.Value,
                value => DeliveryOptionDescription.Create(value).Value
            );

        builder
            .Property(p => p.Price)
            .HasConversion(p => p.Cents, value => Money.FromCents(value).Value)
            .IsRequired();
    }
}
