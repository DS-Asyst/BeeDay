using BeeDay.Application.Features.Projects.Commands;
using BeeDay.Application.Features.Projects.Handlers;
using BeeDay.Application.Features.Projects.Requests;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Application.Tests;

public sealed class ProjectHandlersTests
{
    [Fact]
    public async Task CreateProject_AssignsRequestFieldsAndOwnerToTheCurrentUser()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var handler = new CreateProjectCommandHandler(repository.Projects, new FakeCurrentUserContext(user.Id));

        await handler.Handle(
            new CreateProjectCommand(new SaveProjectRequest("Kitchen renovation", "Full remodel", "#5247F9", new DateOnly(2026, 12, 1), Archived: false)),
            TestContext.Current.CancellationToken);

        var project = Assert.Single(repository.ProjectsData);
        Assert.Equal("Kitchen renovation", project.Title);
        Assert.Equal(user.Id, project.UserId);
        Assert.False(project.Archived);
    }

    [Fact]
    public async Task UpdateProject_ChangesFieldsForTheOwningUser()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var context = new FakeCurrentUserContext(user.Id);
        var project = Project.Create("Original", "Original description");
        project.AssignOwner(user.Id);
        repository.ProjectsData.Add(project);

        var handler = new UpdateProjectCommandHandler(repository.Projects, context);
        await handler.Handle(
            new UpdateProjectCommand(project.Id, new SaveProjectRequest("Renamed", "Updated description", "#000000", null, Archived: true)),
            TestContext.Current.CancellationToken);

        Assert.Equal("Renamed", project.Title);
        Assert.True(project.Archived);
    }

    [Fact]
    public async Task UpdateProject_RejectsAnotherUsersProject()
    {
        var repository = new FakeUnitOfWork();
        var current = CreateCurrentUser(repository);
        var other = User.Create("Other", "other@beeday.invalid");
        repository.UsersData.Add(other);
        var project = Project.Create("Private", null);
        project.AssignOwner(other.Id);
        repository.ProjectsData.Add(project);

        var handler = new UpdateProjectCommandHandler(repository.Projects, new FakeCurrentUserContext(current.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new UpdateProjectCommand(project.Id, new SaveProjectRequest("Hijacked", "", "#000000", null, Archived: false)),
            TestContext.Current.CancellationToken));
        Assert.Equal("Private", project.Title);
    }

    [Fact]
    public async Task DeleteProject_RemovesTheOwningUsersProject()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var project = Project.Create("Disposable", null);
        project.AssignOwner(user.Id);
        repository.ProjectsData.Add(project);

        var handler = new DeleteProjectCommandHandler(repository.Projects, new FakeCurrentUserContext(user.Id));
        await handler.Handle(new DeleteProjectCommand(project.Id), TestContext.Current.CancellationToken);

        Assert.Empty(repository.ProjectsData);
    }

    [Fact]
    public async Task DeleteProject_RejectsAnotherUsersProject()
    {
        var repository = new FakeUnitOfWork();
        var current = CreateCurrentUser(repository);
        var other = User.Create("Other", "other@beeday.invalid");
        repository.UsersData.Add(other);
        var project = Project.Create("Private", null);
        project.AssignOwner(other.Id);
        repository.ProjectsData.Add(project);

        var handler = new DeleteProjectCommandHandler(repository.Projects, new FakeCurrentUserContext(current.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new DeleteProjectCommand(project.Id), TestContext.Current.CancellationToken));
        Assert.Single(repository.ProjectsData);
    }

    private static User CreateCurrentUser(FakeUnitOfWork repository)
    {
        var user = User.Create("Test User", $"{Guid.NewGuid():N}@beeday.invalid");
        repository.UsersData.Add(user);
        return user;
    }
}
