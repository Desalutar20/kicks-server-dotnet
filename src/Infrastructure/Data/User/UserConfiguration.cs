using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Role = Domain.User.Role;

namespace Infrastructure.Data.User;

internal sealed class UserConfiguration : IEntityTypeConfiguration<DomainUser>
{
    public void Configure(EntityTypeBuilder<DomainUser> builder)
    {
        var roles = string.Join(", ", Enum.GetNames<Role>().Select(r => $"'{r.ToLower()}'"));
        var genders = string.Join(", ", Enum.GetNames<Gender>().Select(g => $"'{g.ToLower()}'"));

        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint("CK_role", $"role IN ({roles})");
            table.HasCheckConstraint("CK_gender", $"gender IN ({genders})");
        });

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.GoogleId).IsUnique();
        builder.HasIndex(x => x.FacebookId).IsUnique();

        builder.Property(x => x.Id)
               .HasConversion(
                   id => id.Value,
                   value => new UserId(value)
               )
               .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();


        builder.Property(x => x.FirstName)
               .HasConversion(
                   firstName => firstName != null ? firstName.Value.Value : null,
                   value => value != null
                       ? FirstName.Create(value).Value
                       : null
               )
               .HasMaxLength(FirstName.MaxLength);

        builder.Property(x => x.LastName)
               .HasConversion(
                   lastName => lastName != null ? lastName.Value.Value : null,
                   value => value != null
                       ? LastName.Create(value).Value
                       : null
               )
               .HasMaxLength(LastName.MaxLength);


        builder.Property(x => x.Email)
               .HasConversion(email => email.Value, value => Email.Create(value).Value)
               .HasMaxLength(Email.MaxLength)
               .IsRequired();


        builder.Property(x => x.HashedPassword)
               .HasConversion(
                   hashedPassword => hashedPassword != null ? hashedPassword.Value.Value : null,
                   value => value != null
                       ? HashedPassword.Create(value).Value
                       : null
               )
               .HasMaxLength(HashedPassword.MaxLength);


        builder.Property(x => x.Gender)
               .HasConversion(
                   gender => gender.HasValue ? gender.Value.ToString().ToLower() : null,
                   value => value != null
                       ? Enum.Parse<Gender>(value, true)
                       : null
               )
               .IsRequired(false);

        builder.Property(x => x.Role)
               .HasConversion(
                   role => role.ToString().ToLower(),
                   value => Enum.Parse<Role>(value, true)
               )
               .IsRequired().HasDefaultValue(Role.Regular);


        builder.Property(x => x.GoogleId)
               .HasConversion(
                   googleId => googleId != null ? googleId.Value.Value : null,
                   value => value != null
                       ? ProviderId.Create(value).Value
                       : null
               )
               .HasMaxLength(ProviderId.MaxLength);


        builder.Property(x => x.FacebookId)
               .HasConversion(
                   facebookId => facebookId != null ? facebookId.Value.Value : null,
                   value => value != null
                       ? ProviderId.Create(value).Value
                       : null
               )
               .HasMaxLength(ProviderId.MaxLength);


        builder.Property(x => x.IsVerified).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsBanned).IsRequired().HasDefaultValue(false);
    }
}