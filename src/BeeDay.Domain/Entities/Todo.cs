using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed class Todo : Activity
{
    public Guid ProjectId { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public static Todo Create(Guid projectId, string title, string? description, DateOnly? dueDate, ActivityAttribute? attribute = null)
    {
        var todo = new Todo();
        todo.Update(projectId, title, description, dueDate, attribute);
        return todo;
    }

    public void Update(Guid projectId, string title, string? description, DateOnly? dueDate, ActivityAttribute? attribute = null)
    {
        if (projectId == Guid.Empty)
        {
            throw new DomainValidationException("ProjectId", "A To-Do must belong to a Project.");
        }

        ProjectId = projectId;
        UpdateDetails(title, description);
        DueDate = dueDate;
        SetAttribute(attribute);
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
