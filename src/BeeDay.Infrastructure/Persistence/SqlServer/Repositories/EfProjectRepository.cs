using BeeDay.Application.Common.Contracts;
using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfProjectRepository : EfRepositoryBase, IProjectRepository
{
    public EfProjectRepository(IDbContextFactory<BeeDayDbContext> contextFactory) : base(contextFactory)
    {
    }

    internal EfProjectRepository(BeeDayDbContext sharedContext) : base(sharedContext)
    {
    }

    public async Task<Project?> GetAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await ProjectsWithOrderedTodos(lease.Context)
            .FirstOrDefaultAsync(project => project.UserId == userId && project.Id == projectId, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await ProjectsWithOrderedTodos(lease.Context)
            .Where(project => project.UserId == userId)
            .OrderBy(project => EF.Property<int>(project, "Position"))
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByTodoIdAsync(Guid userId, Guid todoId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await ProjectsWithOrderedTodos(lease.Context)
            .FirstOrDefaultAsync(
                project => project.UserId == userId && project.Todos.Any(todo => todo.Id == todoId),
                cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

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

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task RemoveAsync(Project project, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // See EfHabitRepository.RemoveAsync for why this re-fetches by Id instead of attaching
        // `project` directly (including why the UserId filter is defense-in-depth, BD30-F053).
        // Todos are not loaded here — deleting the Project row cascades to its Todos at the database
        // level (FK_Todos_Projects_ProjectId, ON DELETE CASCADE), so there is no need to materialize
        // the child rows just to remove them.
        var tracked = await context.Projects.SingleAsync(
            existing => existing.Id == project.Id && existing.UserId == project.UserId, cancellationToken);
        context.Projects.Remove(tracked);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task ReorderAsync(
        Guid userId,
        IReadOnlyList<Guid> orderedProjectIds,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

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

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task ReorderTodosAsync(
        Guid userId,
        Guid projectId,
        IReadOnlyList<Guid> orderedTodoIds,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

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

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task UpdateAsync(
        Guid userId,
        Guid projectId,
        Action<Project> mutation,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // See EfUserRepository.UpdateAsync for why tracked-and-mutated-within-this-call, not a
        // disconnected Save. Project only — no Todo mutation touches the Project itself (see
        // UpdateTodoAsync).
        var project = await context.Projects
            .SingleAsync(existing => existing.UserId == userId && existing.Id == projectId, cancellationToken);
        mutation(project);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task AddTodoAsync(
        Guid userId,
        Guid projectId,
        Todo todo,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        var project = await context.Projects
            .Include(existing => existing.Todos)
            .SingleAsync(existing => existing.UserId == userId && existing.Id == projectId, cancellationToken);

        // See EfHabitRepository.AddAsync — Position has no database default, scoped here by ProjectId.
        var maxPosition = await context.Todos
            .Where(existing => existing.ProjectId == projectId)
            .Select(existing => (int?)EF.Property<int>(existing, "Position"))
            .MaxAsync(cancellationToken);

        // Domain method — assigns todo.ProjectId and touches the Project, both via public Domain API.
        project.AddTodo(todo);

        // context.Todos.Add(todo) explicitly, before touching context.Entry(todo): unlike the bulk
        // AddAsync (where context.Projects.Add(project) cascades Added-tracking through the whole graph,
        // including any Todos already attached), `project` here was loaded via a query, not Add() — so
        // `todo`, freshly appended to project.Todos, is not tracked yet. Calling context.Entry(todo)
        // first (to set its Position) would attach it as Unchanged (EF Core's default for an
        // untracked entity reached via Entry, not Add), turning the intended INSERT into a bogus
        // UPDATE that matches zero rows — confirmed by this exact failure while writing this method's
        // test.
        context.Todos.Add(todo);
        context.Entry(todo).Property("Position").CurrentValue = (maxPosition ?? -1) + 1;

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task UpdateTodoAsync(
        Guid userId,
        Guid todoId,
        Action<Todo> mutation,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // Todo only, not the parent Project — confirmed reading Todo.cs/Activity.cs that no Todo
        // mutation (Update/ToggleCompletion) touches its owning Project; only Project.AddTodo/RemoveTodo
        // do that (see AddTodoAsync/RemoveTodoAsync).
        var todo = await context.Todos
            .SingleAsync(existing => existing.UserId == userId && existing.Id == todoId, cancellationToken);
        mutation(todo);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task RemoveTodoAsync(Guid userId, Guid todoId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // Loaded through its owning Project (with Todos tracked) so Project.RemoveTodo — the Domain
        // method — can run for real: it removes the Todo from the tracked collection (EF Core then
        // deletes the row) and touches the Project, exactly like the JSON path today.
        var project = await context.Projects
            .Include(existing => existing.Todos)
            .SingleAsync(
                existing => existing.UserId == userId && existing.Todos.Any(todo => todo.Id == todoId),
                cancellationToken);

        project.RemoveTodo(todoId);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task MoveTodoAsync(
        Guid userId,
        Guid todoId,
        Guid destinationProjectId,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        var sourceProject = await context.Projects
            .Include(existing => existing.Todos)
            .SingleAsync(
                existing => existing.UserId == userId && existing.Todos.Any(todo => todo.Id == todoId),
                cancellationToken);

        var destinationProject = await context.Projects
            .Include(existing => existing.Todos)
            .SingleAsync(
                existing => existing.UserId == userId && existing.Id == destinationProjectId,
                cancellationToken);

        var todo = sourceProject.Todos.Single(existing => existing.Id == todoId);

        // Both calls are the same public Domain API AddTodoAsync/RemoveTodoAsync already use — Todo's
        // internal AssignTo(projectId) only runs from inside BeeDay.Domain (via Project.AddTodo),
        // Infrastructure never calls it directly.
        sourceProject.RemoveTodo(todoId);
        destinationProject.AddTodo(todo);

        var maxPosition = await context.Todos
            .Where(existing => existing.ProjectId == destinationProjectId)
            .Select(existing => (int?)EF.Property<int>(existing, "Position"))
            .MaxAsync(cancellationToken);
        context.Entry(todo).Property("Position").CurrentValue = (maxPosition ?? -1) + 1;

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    private static IQueryable<Project> ProjectsWithOrderedTodos(BeeDayDbContext context) =>
        context.Projects
            .AsNoTracking()
            .Include(project => project.Todos.OrderBy(todo => EF.Property<int>(todo, "Position")));
}
