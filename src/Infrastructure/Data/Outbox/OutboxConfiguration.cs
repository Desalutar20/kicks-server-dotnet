using Domain.Outbox;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Outbox;

internal sealed class OutboxConfiguration : IEntityTypeConfiguration<DomainOutbox>
{
    public void Configure(EntityTypeBuilder<DomainOutbox> builder)
    {
        var types = string.Join(", ", Enum.GetNames<OutboxType>().Select(r => $"'{r.ToLower()}'"));

        builder.ToTable(
            "outbox",
            table =>
            {
                table.HasCheckConstraint("CK_type", $"type IN ({types})");
            }
        );

        builder.HasKey(x => x.Id);

        builder
            .HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("idx_outbox_processed_at_null")
            .HasFilter("processed_at IS NULL");

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new OutboxId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnType("timestamptz");

        builder
            .Property(x => x.Data)
            .HasConversion(data => data.Value, value => NonEmptyString.Create(value).Value)
            .HasColumnType("jsonb")
            .IsRequired();

        builder
            .Property(x => x.Type)
            .HasConversion(
                role => role.ToString().ToLower(),
                value => Enum.Parse<OutboxType>(value, true)
            )
            .IsRequired();
    }
}
