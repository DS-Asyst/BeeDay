using LevelUp.Domain.Entities;
using LevelUp.Domain.Experience;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class ExperienceEntryConfiguration : IEntityTypeConfiguration<ExperienceEntry>
{
    public void Configure(EntityTypeBuilder<ExperienceEntry> builder)
    {
        builder.ToTable("ExperienceEntries", table =>
        {
            table.HasCheckConstraint("CK_ExperienceEntries_SourceType", "[SourceType] BETWEEN 0 AND 6");
            table.HasCheckConstraint("CK_ExperienceEntries_RewardType", "[RewardType] = 0");
        });

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.RewardType)
            .HasConversion<byte>();

        // Computed pass-through/alias properties (get-only, no backing field) — EF Core does not skip
        // these by convention on its own; left unignored, model validation fails with "No backing
        // field could be found ... and the property does not have a setter" (confirmed by
        // LevelUpDbContextTests during this Sprint). The real, stored values live on the owned
        // ExperienceSource below.
        builder.Ignore(entry => entry.SourceType);
        builder.Ignore(entry => entry.SourceId);
        builder.Ignore(entry => entry.OccurredAtUtc);

        // ExperienceSource has no identity of its own in Domain (no Id) — a single value embedded in
        // ExperienceEntry.Source. Mapped as an EF Core complex type (not an owned entity type): a
        // complex type is not a separate entity in the model at all — no key, no shadow FK, always
        // inline on the owner's table — which is a more precise fit than OwnsOne for a pure value
        // object, and avoids two real problems OwnsOne caused here: the RowVersion loop's owned-type
        // exclusion, and HasIndex(string[]) being unable to resolve a dotted path into an owned entity
        // type's properties. Columns match 01-relational-model.md §2 "ExperienceEntries" exactly
        // (SourceType/SourceId/SourceDescription live inline on the same row).
        builder.ComplexProperty(entry => entry.Source, source =>
        {
            source.Property(experienceSource => experienceSource.Type)
                .HasColumnName("SourceType")
                .HasConversion<byte>();

            source.Property(experienceSource => experienceSource.ReferenceId)
                .HasColumnName("SourceId");

            source.Property(experienceSource => experienceSource.Description)
                .HasColumnName("SourceDescription")
                .HasMaxLength(ExperienceSource.MaximumDescriptionLength)
                .IsRequired();
        });

        // Dedup rule read directly from UserExperience.EnsureValidState(): a duplicate grant is any
        // entry sharing (UserId, SourceType, SourceId, RewardType) with an existing one — except
        // SourceType = 0 (Habit), which Domain exempts because a habit can legitimately repeat the same
        // origin. Filtered unique index, not a stored IdempotencyKey (01-relational-model.md §2
        // "ExperienceEntries" — neither IdempotencyKey nor MetadataJson exist in Domain).
        //
        // NOT declared here via Fluent API — confirmed, by exhausting every available surface during
        // this Sprint, that EF Core cannot express an index spanning both an entity's own properties
        // and a nested complex type's properties: the lambda HasIndex overload rejects nested member
        // access ("entry.Source.Type"); the string-array overload cannot resolve a dotted path
        // ("Source.Type") and resolves the bare name "SourceId"/"SourceType" against ExperienceEntry's
        // own ignored computed pass-through properties of the same name instead; and even the raw
        // IMutableEntityType.AddIndex metadata API explicitly rejects properties "not declared on the
        // entity type" when they belong to a complex type. This is a confirmed EF Core limitation, not
        // a mapping choice — the index is instead added via raw SQL directly in the InitialCreate
        // migration (see Migrations/*_InitialCreate.cs), the standard, well-precedented workaround for
        // exactly this scenario. Empirically verified against a real SQL Server LocalDB instance
        // (Sprint 14.3 closeout): `dotnet ef database update` created the index without error and
        // `sys.indexes`/`filter_definition` confirmed it matches exactly
        // `([SourceId] IS NOT NULL AND [SourceType]<>(0))` — this is not a theoretical workaround, it
        // is a proven one.
        builder.HasIndex(entry => new { entry.UserId, entry.GrantedAtUtc })
            .HasDatabaseName("IX_ExperienceEntries_User_Time");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(entry => entry.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ExperienceEntries_Users_UserId");

        // SourceId is a polymorphic reference by design (Habits/RecurringTasks/Todos/Projects
        // depending on SourceType, or null for Reading/Manual/System) — no FK, and none should ever be
        // added here. ExperienceEntries is history: a copy of what happened at the moment XP was
        // granted, not a live reference to the originating aggregate (Aggregate Map §7). See
        // 01-relational-model.md §2 "ExperienceEntries" for the full rationale.
    }
}
