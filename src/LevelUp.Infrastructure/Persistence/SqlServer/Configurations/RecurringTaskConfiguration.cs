using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class RecurringTaskConfiguration : IEntityTypeConfiguration<RecurringTask>
{
    public void Configure(EntityTypeBuilder<RecurringTask> builder)
    {
        builder.ToTable("RecurringTasks", table =>
        {
            table.HasCheckConstraint("CK_RecurringTasks_Attribute", "[Attribute] IN (1, 2, 3, 4) OR [Attribute] IS NULL");
            table.HasCheckConstraint("CK_RecurringTasks_Repeat", "[Repeat] IN (0, 1, 2, 3)");
        });

        // No explicit HasKey here — see HabitConfiguration for why (TPC key lives on the root type).
        builder.ConfigureActivityProperties();

        builder.Property(task => task.Repeat)
            .HasConversion<byte>()
            .HasDefaultValue(TaskRepeat.Daily);

        // Infrastructure-only shadow property — see HabitConfiguration for rationale.
        builder.Property<int>("Position");

        builder.HasIndex("UserId", "Position")
            .HasDatabaseName("IX_RecurringTasks_User_Position");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(task => task.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RecurringTasks_Users_UserId");
    }
}
