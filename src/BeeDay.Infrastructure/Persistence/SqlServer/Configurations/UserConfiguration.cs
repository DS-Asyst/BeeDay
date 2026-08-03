using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", table =>
        {
            table.HasCheckConstraint("CK_Users_Language", "[Language] IN (0, 1)");
            table.HasCheckConstraint("CK_Users_Theme", "[Theme] IN (0, 1, 2)");
        });

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .HasDefaultValue(true);

        builder.Property(user => user.IsEmailConfirmed)
            .HasDefaultValue(false);

        builder.Property(user => user.HasCompletedOnboarding)
            .HasDefaultValue(false);

        builder.Property(user => user.Nickname)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(user => user.Avatar)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.Language)
            .HasConversion<byte>()
            .HasDefaultValue(UserLanguage.English);

        builder.Property(user => user.Theme)
            .HasConversion<byte>()
            .HasDefaultValue(UserTheme.System);

        builder.Property(user => user.SessionVersion)
            .HasDefaultValue(1);

        // Computed properties (get-only, no backing field) — must be explicitly ignored or model
        // validation fails ("No backing field could be found ... and the property does not have a
        // setter"). HasProfile derives from Nickname; Profile is a read view assembled from other
        // User/UserExperience fields on every call, not a separately persisted type
        // (Entities/Profile.cs has no Id, no Entity base).
        builder.Ignore(user => user.HasProfile);
        builder.Ignore(user => user.Profile);

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("UX_Users_Email");

        // Nickname is "" (never null) until profile completion, and Domain allows that empty value to
        // repeat across every user that has not completed their profile — a plain unique index would
        // reject the second user to sign up. Filtered to exclude the empty sentinel, relying on SQL
        // Server's default case-insensitive collation for uniqueness (01-relational-model.md §5.3) —
        // no NormalizedNickname column.
        builder.HasIndex(user => user.Nickname)
            .IsUnique()
            .HasFilter("[Nickname] <> N''")
            .HasDatabaseName("UX_Users_Nickname");

        // UserExperience has no identity of its own in Domain (no Id, no UserId) — it is a single
        // value embedded in User.Experience. Owned in a separate table sharing User's primary key,
        // which also gives it EF Core's fixed cascade-delete-with-owner behavior — exactly
        // "User 1 -- 1 UserExperience (CASCADE)" per 01-relational-model.md §3.
        builder.OwnsOne(user => user.Experience, experience =>
        {
            experience.ToTable("UserExperience");
            experience.WithOwner().HasForeignKey("UserId");
            experience.HasKey("UserId");

            experience.Property(userExperience => userExperience.TotalExperience)
                .HasDefaultValue(0L);

            // Computed properties (get-only, derived from TotalExperience via ExperienceCurve) — must
            // be explicitly ignored, same reason as User.HasProfile/Profile above.
            experience.Ignore(userExperience => userExperience.CurrentLevel);
            experience.Ignore(userExperience => userExperience.CurrentLevelExperience);
            experience.Ignore(userExperience => userExperience.ExperienceRequiredForCurrentLevel);
            experience.Ignore(userExperience => userExperience.ExperienceForNextLevel);

            // ExperienceEntry is its own top-level entity, related to User directly via
            // ExperienceEntries.UserId (01-relational-model.md §3: "User 1 -- * ExperienceEntries", not
            // "UserExperience 1 -- * ExperienceEntries") — mapping Entries here would duplicate/
            // conflict with that already-correct relationship.
            experience.Ignore(userExperience => userExperience.Entries);

            // Added here, not by LevelUpDbContext's global RowVersion loop: that loop cannot touch
            // owned types at all (modelBuilder.Entity(Type) conflicts with an already-owned
            // classification), so each owned type that needs RowVersion configures it directly.
            // 01-relational-model.md §2 "UserExperience" lists RowVersion for this table.
            experience.Property<byte[]>("RowVersion").IsRowVersion();
        });
    }
}
