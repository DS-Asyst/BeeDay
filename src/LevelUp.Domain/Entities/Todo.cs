using System.Text.Json.Serialization;

namespace LevelUp.Domain.Entities;

public sealed class Todo : Activity
{
    [JsonInclude]
    public DateOnly? DueDate { get; private set; }

    public static Todo Create(string title, string? description, DateOnly? dueDate)
    {
        var todo = new Todo();
        todo.Update(title, description, dueDate);
        return todo;
    }

    public void Update(string title, string? description, DateOnly? dueDate)
    {
        UpdateDetails(title, description);
        DueDate = dueDate;
    }
}
