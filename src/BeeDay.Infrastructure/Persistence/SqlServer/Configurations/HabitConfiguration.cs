using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.ToTable("Habits", table =>
        {
            table.HasCheckConstraint("CK_Habits_Attribute", "[Attribute] IN (1, 2, 3, 4) OR [Attribute] IS NULL");
            table.HasCheckConstraint("CK_Habits_Direction", "[Direction] IN (0, 1, 2)");
            table.HasCheckConstraint("CK_Habits_Difficulty", "[Difficulty] IN (0, 1, 2, 3)");
            table.HasCheckConstraint("CK_Habits_ResetCounter", "[ResetCounter] IN (0, 1, 2)");
        });

        // No explicit HasKey here: under TPC the primary key is configured once on the hierarchy's
        // root type (Activity), never on a derived type — EF Core already discovers Entity.Id as that
        // key by convention.
        builder.ConfigureActivityProperties();

        builder.Property(habit => habit.Direction)
            .HasConversion<byte>()
            .HasDefaultValue(HabitDirection.Both);

        builder.Property(habit => habit.Difficulty)
            .HasConversion<byte>()
            .HasDefaultValue(HabitDifficulty.Easy);

        builder.Property(habit => habit.ResetCounter)
            .HasConversion<byte>()
            .HasDefaultValue(HabitResetCounter.Daily);

        builder.Property(habit => habit.PositiveCount)
            .HasDefaultValue(0);

        builder.Property(habit => habit.NegativeCount)
            .HasDefaultValue(0);

        // Infrastructure-only shadow property — no Domain field. 01-relational-model.md §5.2: SQL
        // Server has no implicit row order, so persisting the existing drag-to-reorder behavior needs
        // an explicit ordinal, scoped by UserId for Habits.
        builder.Property<int>("Position");

        builder.HasIndex("UserId", "Position")
            .HasDatabaseName("IX_Habits_User_Position");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(habit => habit.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Habits_Users_UserId");
    }
}
