using LevelUp.Domain.Attributes;
using LevelUp.Services.Projects;
using LevelUp.Services.Todos;
using Xunit;

namespace LevelUp.Tests;

public sealed class ProjectTodoServiceTests
{
    [Fact]
    public void CompletedTodo_CannotBeDeleted()
    {
        ProjectService projectService = new();
        var project = projectService.CreateProject(
            "LevelUp",
            string.Empty,
            AttributeType.Intelligence);

        ProjectTodoService service = new();
        var todo = service.Create(project, "Refactor domain", string.Empty);

        service.Activate(todo);
        service.Complete(todo);

        Assert.Throws<InvalidOperationException>(() => service.Delete(todo.Id));
    }
}
