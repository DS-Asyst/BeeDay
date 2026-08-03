using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.ToTable("Todos", table =>
            table.HasCheckConstraint("CK_Todos_Attribute", "[Attribute] IN (1, 2, 3, 4) OR [Attribute] IS NULL"));

        // No explicit HasKey here — see HabitConfiguration for why (TPC key lives on the root type).
        builder.ConfigureActivityProperties();

        builder.Property(todo => todo.ProjectId)
            .IsRequired();

        builder.Property(todo => todo.DueDate);

        // Infrastructure-only shadow property — scoped by ProjectId, not UserId: reordering Todos is
        // always "within the same Project" (01-relational-model.md §2 "Todos", Persistence Map §2.5),
        // never a global-to-user ordinal like the other three Activity-derived tables.
        builder.Property<int>("Position");

        builder.HasIndex("ProjectId", "Position")
            .HasDatabaseName("IX_Todos_Project_Position");

        builder.HasIndex(todo => new { todo.UserId, todo.Id })
            .HasDatabaseName("IX_Todos_User");

        // NO ACTION, not CASCADE: deleting a Todo when its User is deleted already happens
        // transitively via Users -> Projects (CASCADE) -> Todos (CASCADE, configured in
        // ProjectConfiguration). A second CASCADE path directly from Users to Todos would make SQL
        // Server reject the schema at creation time ("may cause cycles or multiple cascade paths" —
        // error 1785). 01-relational-model.md §5.1.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(todo => todo.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Todos_Users_UserId");

        // ProjectId's FK/CASCADE is configured from the owning side in ProjectConfiguration
        // (HasMany(project => project.Todos)) — configuring it again here would be redundant.
    }
}
