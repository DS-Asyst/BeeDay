using BeeDay.Application.Features.Todos.Commands;
using BeeDay.Application.Features.Todos.Handlers;
using BeeDay.Application.Features.Todos.Requests;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Application.Tests;

public sealed class TodoHandlersTests
{
    [Fact]
    public async Task CreateTodo_AddsTodoToTheOwningProject()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var project = AddProject(repository, user.Id, "Home");
        var handler = new CreateTodoCommandHandler(repository.Projects, new FakeCurrentUserContext(user.Id));

        await handler.Handle(
            new CreateTodoCommand(new SaveTodoRequest(project.Id, "Buy paint", "", null)),
            TestContext.Current.CancellationToken);

        var todo = Assert.Single(project.Todos);
        Assert.Equal("Buy paint", todo.Title);
        Assert.Equal(user.Id, todo.UserId);
    }

    [Fact]
    public async Task CreateTodo_RejectsProjectOwnedByAnotherUser()
    {
        var repository = new FakeUnitOfWork();
        var current = CreateCurrentUser(repository);
        var other = User.Create("Other", "other@beeday.invalid");
        repository.UsersData.Add(other);
        var project = AddProject(repository, other.Id, "Private");
        var handler = new CreateTodoCommandHandler(repository.Projects, new FakeCurrentUserContext(current.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new CreateTodoCommand(new SaveTodoRequest(project.Id, "Intrude", "", null)),
            TestContext.Current.CancellationToken));
        Assert.Empty(project.Todos);
    }

    [Fact]
    public async Task UpdateTodo_ChangesFieldsWithinTheSameProject()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var project = AddProject(repository, user.Id, "Home");
        var todo = Todo.Create(project.Id, "Original", "", null);
        project.AddTodo(todo);

        var handler = new UpdateTodoCommandHandler(repository.Projects, repository, new FakeCurrentUserContext(user.Id));
        await handler.Handle(
            new UpdateTodoCommand(todo.Id, new SaveTodoRequest(project.Id, "Renamed", "Updated", new DateOnly(2026, 9, 1))),
            TestContext.Current.CancellationToken);

        Assert.Equal("Renamed", todo.Title);
        Assert.Equal(new DateOnly(2026, 9, 1), todo.DueDate);
        Assert.Equal(project.Id, todo.ProjectId);
    }

    [Fact]
    public async Task UpdateTodo_MovesTodoToADifferentProjectOfTheSameUser()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var source = AddProject(repository, user.Id, "Source");
        var destination = AddProject(repository, user.Id, "Destination");
        var todo = Todo.Create(source.Id, "Movable", "", null);
        source.AddTodo(todo);

        var handler = new UpdateTodoCommandHandler(repository.Projects, repository, new FakeCurrentUserContext(user.Id));
        await handler.Handle(
            new UpdateTodoCommand(todo.Id, new SaveTodoRequest(destination.Id, "Movable", "", null)),
            TestContext.Current.CancellationToken);

        Assert.Empty(source.Todos);
        var moved = Assert.Single(destination.Todos);
        Assert.Equal(destination.Id, moved.ProjectId);
    }

    [Fact]
    public async Task DeleteTodo_RemovesTodoFromTheOwningProject()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var project = AddProject(repository, user.Id, "Home");
        var todo = Todo.Create(project.Id, "Disposable", "", null);
        project.AddTodo(todo);

        var handler = new DeleteTodoCommandHandler(repository.Projects, new FakeCurrentUserContext(user.Id));
        await handler.Handle(new DeleteTodoCommand(todo.Id), TestContext.Current.CancellationToken);

        Assert.Empty(project.Todos);
    }

    [Fact]
    public async Task DeleteTodo_RejectsTodoOwnedByAnotherUser()
    {
        var repository = new FakeUnitOfWork();
        var current = CreateCurrentUser(repository);
        var other = User.Create("Other", "other@beeday.invalid");
        repository.UsersData.Add(other);
        var project = AddProject(repository, other.Id, "Private");
        var todo = Todo.Create(project.Id, "Private todo", "", null);
        project.AddTodo(todo);

        var handler = new DeleteTodoCommandHandler(repository.Projects, new FakeCurrentUserContext(current.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new DeleteTodoCommand(todo.Id), TestContext.Current.CancellationToken));
        Assert.Single(project.Todos);
    }

    private static User CreateCurrentUser(FakeUnitOfWork repository)
    {
        var user = User.Create("Test User", $"{Guid.NewGuid():N}@beeday.invalid");
        repository.UsersData.Add(user);
        return user;
    }

    private static Project AddProject(FakeUnitOfWork repository, Guid userId, string name)
    {
        var project = Project.Create(name, null);
        project.AssignOwner(userId);
        repository.ProjectsData.Add(project);
        return project;
    }
}
