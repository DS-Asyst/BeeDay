using LevelUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("UserTokens", table =>
            table.HasCheckConstraint("CK_UserTokens_Type", "[Type] IN (1, 2)"));

        builder.HasKey(token => token.Id);

        builder.Property(token => token.Type)
            .HasConversion<byte>();

        builder.Property(token => token.TokenHash)
            .HasMaxLength(200)
            .IsRequired();

        // Computed properties (get-only, derived from UsedAtUtc/RevokedAtUtc) — must be explicitly
        // ignored, same reason as elsewhere in this Sprint's configurations.
        builder.Ignore(token => token.IsUsed);
        builder.Ignore(token => token.IsRevoked);

        builder.HasIndex(token => new { token.TokenHash, token.Type })
            .IsUnique()
            .HasDatabaseName("UX_UserTokens_Hash");

        builder.HasIndex(token => new { token.UserId, token.Type, token.ExpiresAtUtc })
            .HasDatabaseName("IX_UserTokens_User");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_UserTokens_Users_UserId");
    }
}
