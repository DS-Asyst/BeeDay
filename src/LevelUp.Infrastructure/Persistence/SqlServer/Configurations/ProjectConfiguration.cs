using LevelUp.Domain.Entities;
using LevelUp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", table =>
        {
            table.HasCheckConstraint("CK_Projects_Attribute", "[Attribute] IN (1, 2, 3, 4) OR [Attribute] IS NULL");
            table.HasCheckConstraint(
                "CK_Projects_Color",
                "[Color] LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'");
        });

        // No explicit HasKey here — see HabitConfiguration for why (TPC key lives on the root type).
        builder.ConfigureActivityProperties();

        builder.Property(project => project.Color)
            .HasMaxLength(7)
            .IsFixedLength()
            .IsRequired()
            .HasDefaultValue(ProjectColor.Default);

        builder.Property(project => project.ExpectedDate);

        builder.Property(project => project.Archived)
            .HasDefaultValue(false);

        // Infrastructure-only shadow property — see HabitConfiguration for rationale.
        builder.Property<int>("Position");

        builder.HasIndex("UserId", "Position")
            .HasDatabaseName("IX_Projects_User_Position");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(project => project.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Projects_Users_UserId");

        // Status and the rest of these read-only projections are Project's own computed properties
        // (no setter, no backing field) — never mapped, per 01-relational-model.md §2 "Projects". Must
        // be explicitly ignored, or model validation fails ("No backing field could be found ... and
        // the property does not have a setter") — same reason as elsewhere in this Sprint's
        // configurations. Completed is handled differently — see ActivityConfigurationExtensions — it
        // cannot be ignored per derived type under TPC (EF Core requires that at the root, Activity).
        builder.Ignore(project => project.Name);
        builder.Ignore(project => project.TotalTodos);
        builder.Ignore(project => project.PendingTodos);
        builder.Ignore(project => project.CompletedTodos);
        builder.Ignore(project => project.ProgressPercentage);
        builder.Ignore(project => project.Progress);
        builder.Ignore(project => project.LastUpdatedAtUtc);
        builder.Ignore(project => project.NextTodo);
        builder.Ignore(project => project.Status);

        // Todo containment: Project owns the lifecycle of its Todos (Aggregate Map §2.5) — deleting a
        // Project must delete its Todos. Todos.UserId is configured separately, in TodoConfiguration,
        // as ON DELETE NO ACTION (01-relational-model.md §5.1 — avoids a second, redundant cascade path
        // from Users converging on Todos alongside this one).
        builder.HasMany(project => project.Todos)
            .WithOne()
            .HasForeignKey(todo => todo.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Todos_Projects_ProjectId");
    }
}
