using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using Xunit;

namespace BeeDay.Domain.Tests;

public sealed class ProjectTests
{
    [Fact]
    public void Empty_project_is_planned_with_zero_progress()
    {
        var project = Project.Create("Project", null);

        Assert.Equal(0, project.TotalTodos);
        Assert.Equal(0, project.PendingTodos);
        Assert.Equal(0, project.CompletedTodos);
        Assert.Equal(0m, project.ProgressPercentage);
        Assert.Equal(ProjectStatus.Planned, project.Status);
        Assert.False(project.Completed);
        Assert.Null(project.NextTodo);
    }

    [Fact]
    public void Completing_all_todos_completes_project_automatically()
    {
        var project = Project.Create("Project", null);
        var todo = Todo.Create(project.Id, "Todo", null, null);
        project.AddTodo(todo);

        todo.ToggleCompletion();

        Assert.Equal(1, project.TotalTodos);
        Assert.Equal(0, project.PendingTodos);
        Assert.Equal(1, project.CompletedTodos);
        Assert.Equal(100m, project.ProgressPercentage);
        Assert.Equal(ProjectStatus.Completed, project.Status);
        Assert.True(project.Completed);
        Assert.Null(project.NextTodo);
    }

    [Fact]
    public void Adding_pending_todo_to_completed_project_returns_it_to_in_progress()
    {
        var project = Project.Create("Project", null);
        var completed = Todo.Create(project.Id, "Completed", null, null);
        project.AddTodo(completed);
        completed.ToggleCompletion();

        project.AddTodo(Todo.Create(project.Id, "New", null, null));

        Assert.Equal(ProjectStatus.InProgress, project.Status);
        Assert.False(project.Completed);
        Assert.Equal(1, project.PendingTodos);
        Assert.Equal(50m, project.ProgressPercentage);
    }

    [Fact]
    public void Reopening_todo_returns_completed_project_to_in_progress()
    {
        var project = Project.Create("Project", null);
        var first = Todo.Create(project.Id, "First", null, null);
        var second = Todo.Create(project.Id, "Second", null, null);
        project.AddTodo(first);
        project.AddTodo(second);
        first.ToggleCompletion();
        second.ToggleCompletion();

        second.ToggleCompletion();

        Assert.Equal(ProjectStatus.InProgress, project.Status);
        Assert.Equal(1, project.PendingTodos);
        Assert.False(project.Completed);
    }

    [Fact]
    public void Next_todo_prefers_nearest_due_date_then_creation_order()
    {
        var project = Project.Create("Project", null);
        var later = Todo.Create(project.Id, "Later", null, new DateOnly(2026, 8, 10));
        var noDate = Todo.Create(project.Id, "No date", null, null);
        var sooner = Todo.Create(project.Id, "Sooner", null, new DateOnly(2026, 7, 30));
        project.AddTodo(later);
        project.AddTodo(noDate);
        project.AddTodo(sooner);

        Assert.Same(sooner, project.NextTodo);
    }

    [Fact]
    public void Last_update_uses_most_recent_project_or_todo_timestamp()
    {
        var project = Project.Create("Project", null);
        var beforeTodo = project.LastUpdatedAtUtc;
        var todo = Todo.Create(project.Id, "Todo", null, null);
        project.AddTodo(todo);
        todo.ToggleCompletion();

        Assert.True(project.LastUpdatedAtUtc >= beforeTodo);
        Assert.Equal(todo.UpdatedAtUtc, project.LastUpdatedAtUtc);
    }

    [Fact]
    public void Project_cannot_be_completed_manually()
    {
        var project = Project.Create("Project", null);

        var exception = Assert.Throws<InvalidDomainStateException>(project.ToggleCompletion);

        Assert.Contains("cannot be completed manually", exception.Message);
    }
}
