using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfProjectRepository(IDbContextFactory<LevelUpDbContext> contextFactory) : IProjectRepository
{
    public async Task<Project?> GetAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await ProjectsWithOrderedTodos(context)
            .FirstOrDefaultAsync(project => project.UserId == userId && project.Id == projectId, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await ProjectsWithOrderedTodos(context)
            .Where(project => project.UserId == userId)
            .OrderBy(project => EF.Property<int>(project, "Position"))
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByTodoIdAsync(Guid userId, Guid todoId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await ProjectsWithOrderedTodos(context)
            .FirstOrDefaultAsync(
                project => project.UserId == userId && project.Todos.Any(todo => todo.Id == todoId),
                cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // See EfHabitRepository.AddAsync — Position has no database default, the repository assigns
        // the next free slot scoped by UserId (Projects) and, for any Todo already attached to this new
        // Project, scoped by ProjectId (Todos) — both start empty for a brand-new Project, so a plain
        // ordinal by list index is exact, not just an approximation.
        var maxProjectPosition = await context.Projects
            .Where(existing => existing.UserId == project.UserId)
            .Select(existing => (int?)EF.Property<int>(existing, "Position"))
            .MaxAsync(cancellationToken);

        context.Projects.Add(project);
        context.Entry(project).Property("Position").CurrentValue = (maxProjectPosition ?? -1) + 1;

        for (var index = 0; index < project.Todos.Count; index++)
        {
            context.Entry(project.Todos[index]).Property("Position").CurrentValue = index;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Project project, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // See EfHabitRepository.RemoveAsync for why this re-fetches by Id instead of attaching
        // `project` directly. Todos are not loaded here — deleting the Project row cascades to its
        // Todos at the database level (FK_Todos_Projects_ProjectId, ON DELETE CASCADE), so there is no
        // need to materialize the child rows just to remove them.
        var tracked = await context.Projects.SingleAsync(existing => existing.Id == project.Id, cancellationToken);
        context.Projects.Remove(tracked);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderAsync(
        Guid userId,
        IReadOnlyList<Guid> orderedProjectIds,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var projectsById = await context.Projects
            .Where(project => project.UserId == userId)
            .ToDictionaryAsync(project => project.Id, cancellationToken);

        for (var index = 0; index < orderedProjectIds.Count; index++)
        {
            if (projectsById.TryGetValue(orderedProjectIds[index], out var project))
            {
                context.Entry(project).Property("Position").CurrentValue = index;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderTodosAsync(
        Guid userId,
        Guid projectId,
        IReadOnlyList<Guid> orderedTodoIds,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var todosById = await context.Todos
            .Where(todo => todo.UserId == userId && todo.ProjectId == projectId)
            .ToDictionaryAsync(todo => todo.Id, cancellationToken);

        for (var index = 0; index < orderedTodoIds.Count; index++)
        {
            if (todosById.TryGetValue(orderedTodoIds[index], out var todo))
            {
                context.Entry(todo).Property("Position").CurrentValue = index;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Project> ProjectsWithOrderedTodos(LevelUpDbContext context) =>
        context.Projects
            .AsNoTracking()
            .Include(project => project.Todos.OrderBy(todo => EF.Property<int>(todo, "Position")));
}
