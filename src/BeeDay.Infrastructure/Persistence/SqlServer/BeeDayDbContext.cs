using BeeDay.Domain.Entities;
using BeeDay.Domain.Experience;
using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer;

internal sealed class BeeDayDbContext(DbContextOptions<BeeDayDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<UserToken> UserTokens => Set<UserToken>();

    public DbSet<Habit> Habits => Set<Habit>();

    public DbSet<RecurringTask> RecurringTasks => Set<RecurringTask>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Todo> Todos => Set<Todo>();

    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<WalletTag> WalletTags => Set<WalletTag>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<ExperienceEntry> ExperienceEntries => Set<ExperienceEntry>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(19, 2);
        configurationBuilder.Properties<DateTimeOffset>().HavePrecision(7);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Habit/RecurringTask/Project/Todo share the abstract Activity base purely for Domain code
        // reuse, not for shared storage — 01-relational-model.md defines 4 independent, fully-columned
        // tables for them. Scoped to Activity specifically (not the Entity root, as an earlier version
        // of this Sprint's foundation did) — Activity is the only abstract type in this model whose
        // multiple concrete descendants are each independently exposed via their own DbSet, which is
        // exactly the shape that triggers EF Core's default table-per-hierarchy convention (one shared
        // table with a discriminator column, which would not match the approved model). The other 6
        // mapped types (User, UserToken, Wallet, WalletTag, Transaction, ExperienceEntry) never had this
        // ambiguity — confirmed empirically in Sprint 14.2 by BeeDayDbContextTests, which already
        // passed with each of them in its own distinctly-named table without any inheritance
        // configuration at all. TPC (not TPT): the approved model wants each of the four tables fully
        // self-contained, with no shared base table and no joins — exactly what table-per-concrete-type
        // produces. No key here is database-generated (every Id is a Guid assigned by Domain), so TPC's
        // historical key-generation caveat does not apply.
        modelBuilder.Entity<Activity>().UseTpcMappingStrategy();

        // UserExperience and ExperienceSource are mapped as EF Core owned types from
        // UserConfiguration/ExperienceEntryConfiguration respectively (see those files) — both are
        // value-object-shaped in Domain (no Id, no identity property of their own), so neither gets a
        // DbSet of its own. Applied before the RowVersion loop below, deliberately: EF Core's default
        // conventions discover a type reachable from a navigation (e.g. ExperienceEntry.Source) as a
        // plain, non-owned entity type the moment anything calls modelBuilder.Entity(clrType) on it —
        // which the loop below does, for every concrete type, to add the RowVersion shadow property.
        // Doing that before OwnsOne() runs permanently locks the type in as non-owned and OwnsOne()
        // then throws ("already configured as non-owned") — confirmed by BeeDayDbContextTests during
        // this Sprint. Applying the owned-type configuration first avoids the conflict entirely.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BeeDayDbContext).Assembly);

        // RowVersion is not a Domain fact — no entity declares it. It is required by
        // 01-relational-model.md §0 item 14 for every concrete mutable entity, explicitly including
        // Todo (§2 "Todos" table lists RowVersion), with ExperienceEntry as the sole documented
        // exception (append-only, never updated after insert). Added once, globally, as an EF Core
        // shadow property, rather than repeated per Aggregate configuration, for every regular
        // (non-owned) concrete entity type. Abstract types are excluded — GetEntityTypes() also yields
        // the abstract Entity/Activity types, and a shadow property added there would flow down to
        // every descendant (including ExperienceEntry) through inheritance, silently defeating the
        // ExperienceEntry exclusion. Owned types (UserExperience, ExperienceSource) are excluded too,
        // for a different reason: modelBuilder.Entity(Type) — the only overload that accepts a
        // reflected ClrType — cannot be used on a type EF Core has already classified as owned; it
        // throws "cannot be configured as non-owned because it has already been configured as owned"
        // (confirmed by BeeDayDbContextTests during this Sprint). UserExperience still needs
        // RowVersion per 01-relational-model.md §2 "UserExperience" — it is added explicitly inside its
        // own OwnsOne(...) block in UserConfiguration instead. ExperienceSource is inline on the same
        // ExperienceEntries row (no table of its own) and must not get one at all.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(entityType => !entityType.ClrType.IsAbstract
                         && !entityType.IsOwned()
                         && entityType.ClrType != typeof(ExperienceEntry)
                         && entityType.ClrType != typeof(ExperienceSource)))
        {
            modelBuilder.Entity(entityType.ClrType).Property<byte[]>("RowVersion").IsRowVersion();
        }
    }
}
