using LevelUp.Domain.Entities;
using LevelUp.Infrastructure.Persistence.Exceptions;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;

[Collection("EfLocalDb")]
public sealed class EfProjectRepositoryTests : EfLocalDbTestBase
{
    [Fact]
    public async Task AddAsync_ThenGetAsync_IncludesTodosOrderedByInsertion()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var project = CreateProject(userId, "Launch");
        project.AddTodo(CreateTodo(userId, "First todo"));
        project.AddTodo(CreateTodo(userId, "Second todo"));

        await repository.AddAsync(project, cancellationToken);
        var loaded = await repository.GetAsync(userId, project.Id, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(["First todo", "Second todo"], loaded!.Todos.Select(todo => todo.Title));
    }

    [Fact]
    public async Task GetByTodoIdAsync_FindsTheOwningProject()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var project = CreateProject(userId, "Launch");
        var todo = CreateTodo(userId, "First todo");
        project.AddTodo(todo);
        await repository.AddAsync(project, cancellationToken);

        var loaded = await repository.GetByTodoIdAsync(userId, todo.Id, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(project.Id, loaded!.Id);
    }

    [Fact]
    public async Task ReorderAsync_ChangesProjectOrderReturnedByListAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var first = CreateProject(userId, "First");
        var second = CreateProject(userId, "Second");
        await repository.AddAsync(first, cancellationToken);
        await repository.AddAsync(second, cancellationToken);

        await repository.ReorderAsync(userId, [second.Id, first.Id], cancellationToken);
        var listed = await repository.ListAsync(userId, cancellationToken);

        Assert.Equal(["Second", "First"], listed.Select(project => project.Title));
    }

    [Fact]
    public async Task ReorderTodosAsync_ChangesTodoOrderWithinAProject()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var project = CreateProject(userId, "Launch");
        var first = CreateTodo(userId, "First todo");
        var second = CreateTodo(userId, "Second todo");
        project.AddTodo(first);
        project.AddTodo(second);
        await repository.AddAsync(project, cancellationToken);

        await repository.ReorderTodosAsync(userId, project.Id, [second.Id, first.Id], cancellationToken);
        var loaded = await repository.GetAsync(userId, project.Id, cancellationToken);

        Assert.Equal(["Second todo", "First todo"], loaded!.Todos.Select(todo => todo.Title));
    }

    [Fact]
    public async Task RemoveAsync_CascadesToItsTodos()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var project = CreateProject(userId, "Launch");
        var todo = CreateTodo(userId, "First todo");
        project.AddTodo(todo);
        await repository.AddAsync(project, cancellationToken);

        await repository.RemoveAsync(project, cancellationToken);
        var loaded = await repository.GetAsync(userId, project.Id, cancellationToken);
        var byTodo = await repository.GetByTodoIdAsync(userId, todo.Id, cancellationToken);

        Assert.Null(loaded);
        Assert.Null(byTodo);
    }

    [Fact]
    public async Task UpdateAsync_PersistsTheMutation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var project = CreateProject(userId, "Launch");
        await repository.AddAsync(project, cancellationToken);

        await repository.UpdateAsync(userId, project.Id, p => p.SetArchived(true), cancellationToken);

        var loaded = await repository.GetAsync(userId, project.Id, cancellationToken);
        Assert.True(loaded!.Archived);
    }

    [Fact]
    public async Task UpdateAsync_ConcurrentModification_ThrowsConcurrencyConflictException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var project = CreateProject(userId, "Launch");
        await repository.AddAsync(project, cancellationToken);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.UpdateAsync(
            userId,
            project.Id,
            p =>
            {
                using var raceContext = ContextFactory.CreateDbContext();
                var raceProject = raceContext.Projects.Single(existing => existing.Id == project.Id);
                raceProject.SetArchived(true);
                raceContext.SaveChanges();

                p.SetArchived(false);
            },
            cancellationToken));
    }

    [Fact]
    public async Task AddTodoAsync_AddsATodoToAnExistingProject()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var project = CreateProject(userId, "Launch");
        await repository.AddAsync(project, cancellationToken);

        await repository.AddTodoAsync(userId, project.Id, CreateTodo(userId, "First todo"), cancellationToken);

        var loaded = await repository.GetAsync(userId, project.Id, cancellationToken);
        Assert.Equal(["First todo"], loaded!.Todos.Select(todo => todo.Title));
    }

    [Fact]
    public async Task UpdateTodoAsync_PersistsTheMutation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var project = CreateProject(userId, "Launch");
        var todo = CreateTodo(userId, "First todo");
        project.AddTodo(todo);
        await repository.AddAsync(project, cancellationToken);

        await repository.UpdateTodoAsync(userId, todo.Id, t => t.SetFeatured(true), cancellationToken);

        var loaded = await repository.GetByTodoIdAsync(userId, todo.Id, cancellationToken);
        Assert.True(loaded!.Todos.Single(existing => existing.Id == todo.Id).Featured);
    }

    [Fact]
    public async Task RemoveTodoAsync_RemovesTheTodoAndTouchesTheProject()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var project = CreateProject(userId, "Launch");
        var todo = CreateTodo(userId, "First todo");
        project.AddTodo(todo);
        await repository.AddAsync(project, cancellationToken);

        await repository.RemoveTodoAsync(userId, todo.Id, cancellationToken);

        var loaded = await repository.GetAsync(userId, project.Id, cancellationToken);
        Assert.Empty(loaded!.Todos);
        Assert.Null(await repository.GetByTodoIdAsync(userId, todo.Id, cancellationToken));
    }

    [Fact]
    public async Task MoveTodoAsync_MovesTheTodoToTheDestinationProject()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfProjectRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var source = CreateProject(userId, "Source");
        var destination = CreateProject(userId, "Destination");
        var todo = CreateTodo(userId, "First todo");
        source.AddTodo(todo);
        await repository.AddAsync(source, cancellationToken);
        await repository.AddAsync(destination, cancellationToken);

        await repository.MoveTodoAsync(userId, todo.Id, destination.Id, cancellationToken);

        var loadedSource = await repository.GetAsync(userId, source.Id, cancellationToken);
        var loadedDestination = await repository.GetAsync(userId, destination.Id, cancellationToken);
        Assert.Empty(loadedSource!.Todos);
        Assert.Equal(["First todo"], loadedDestination!.Todos.Select(existing => existing.Title));
    }

    private static Project CreateProject(Guid userId, string name)
    {
        var project = Project.Create(name, null);
        project.AssignOwner(userId);
        return project;
    }

    private static Todo CreateTodo(Guid userId, string title)
    {
        // ProjectId here is a placeholder — Project.AddTodo reassigns it via Todo.AssignTo(Id) to the
        // real owning Project's Id. UserId, however, is not set by AddTodo, so it must be assigned here.
        var todo = Todo.Create(Guid.NewGuid(), title, null, null);
        todo.AssignOwner(userId);
        return todo;
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
