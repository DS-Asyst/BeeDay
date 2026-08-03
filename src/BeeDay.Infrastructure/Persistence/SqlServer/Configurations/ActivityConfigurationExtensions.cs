using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Configurations;

/// <summary>
/// Configures the columns Habit/RecurringTask/Project/Todo inherit from the abstract Activity base.
/// Under TPC (BeeDayDbContext.OnModelCreating) each of the four is its own fully self-contained table
/// with no shared base table, so these columns must be configured once per concrete type — this helper
/// avoids repeating the exact same property configuration in all four IEntityTypeConfiguration&lt;T&gt;
/// files. It intentionally does not configure the UserId foreign key: Todo's is ON DELETE NO ACTION
/// while the other three are CASCADE (01-relational-model.md §5.1), so that relationship is configured
/// individually by each entity's own configuration class.
/// </summary>
internal static class ActivityConfigurationExtensions
{
    public static void ConfigureActivityProperties<TActivity>(this EntityTypeBuilder<TActivity> builder)
        where TActivity : Activity
    {
        builder.Property(activity => activity.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(activity => activity.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(activity => activity.Featured)
            .HasDefaultValue(false);

        builder.Property(activity => activity.Attribute)
            .HasConversion<byte?>();

        // Field access mode, deliberately: under TPC, a property inherited from the hierarchy's root
        // (Activity) can only be configured/ignored once, at the root — EF Core rejects any attempt to
        // reconfigure or ignore it per derived type ("must be configured on the root type"). Project
        // overrides Completed with a computed-only implementation (get => Status == ...; protected
        // set { } — no backing field of its own), which breaks EF Core's normal property-accessor-based
        // mapping for that one concrete type. Forcing field access makes EF Core read/write the
        // inherited backing field directly via reflection for all four concrete types uniformly,
        // bypassing every type's accessor (including Project's override) — functionally identical to
        // the default property-accessor behavior for Habit/RecurringTask/Todo (whose own accessors do
        // nothing but read/write that same field), and the only way to keep Project mappable at all.
        // Net effect: Projects ends up with a Completed column that Domain's own Project.Completed
        // getter never reads (it always computes from Todos instead) — a discovered EF Core/TPC
        // limitation, not a Domain fact; see 01-relational-model.md §2 "Projects", which originally
        // called for no Completed column there at all.
        builder.Property(activity => activity.Completed)
            .HasDefaultValue(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(activity => activity.CreatedAtUtc);

        builder.Property(activity => activity.UpdatedAtUtc);
    }
}
