using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class WalletTagConfiguration : IEntityTypeConfiguration<WalletTag>
{
    public void Configure(EntityTypeBuilder<WalletTag> builder)
    {
        builder.ToTable("WalletTags", table =>
            table.HasCheckConstraint(
                "CK_WalletTags_Color",
                "[Color] LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'"));

        builder.HasKey(tag => tag.Id);

        builder.Property(tag => tag.Name)
            .HasMaxLength(WalletTag.MaximumNameLength)
            .IsRequired();

        builder.Property(tag => tag.Color)
            .HasMaxLength(7)
            .IsFixedLength()
            .IsRequired()
            .HasDefaultValue(WalletTag.DefaultColor);

        // Relies on SQL Server's default case-insensitive collation for uniqueness
        // (01-relational-model.md §5.3) — no NormalizedName column.
        builder.HasIndex(tag => new { tag.UserId, tag.Name })
            .IsUnique()
            .HasDatabaseName("UX_WalletTags_User_Name");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(tag => tag.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WalletTags_Users_UserId");
    }
}
