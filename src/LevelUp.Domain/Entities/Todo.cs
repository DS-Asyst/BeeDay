using System.Text.Json.Serialization;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed class Todo : Activity
{
    [JsonInclude]
    public Guid ProjectId { get; private set; }

    [JsonInclude]
    public DateOnly? DueDate { get; private set; }

    public static Todo Create(Guid projectId, string title, string? description, DateOnly? dueDate)
    {
        var todo = new Todo();
        todo.Update(projectId, title, description, dueDate);
        return todo;
    }

    public void Update(Guid projectId, string title, string? description, DateOnly? dueDate)
    {
        if (projectId == Guid.Empty)
        {
            throw new DomainValidationException("ProjectId", "A To-Do must belong to a Project.");
        }

        ProjectId = projectId;
        UpdateDetails(title, description);
        DueDate = dueDate;
    }

    internal void AssignTo(Guid projectId)
    {
        if (projectId == Guid.Empty)
        {
            throw new DomainValidationException("ProjectId", "A To-Do must belong to a Project.");
        }
        ProjectId = projectId;
        Touch();
    }
}
