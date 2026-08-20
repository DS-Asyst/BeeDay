using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Domain.ValueObjects;

namespace BeeDay.Domain.Entities;

public sealed class Project : Activity
{
    private Project() { }

    public string Color { get; private set; } = ProjectColor.Default;

    public DateOnly? ExpectedDate { get; private set; }

    public bool Archived { get; private set; }

    public List<Todo> Todos { get; private set; } = [];

    public string Name => Title;

    public int TotalTodos => Todos.Count;

    public int PendingTodos => Todos.Count(todo => !todo.Completed);

    public int CompletedTodos => Todos.Count(todo => todo.Completed);

    public decimal ProgressPercentage => TotalTodos == 0
        ? 0m
        : decimal.Round(CompletedTodos * 100m / TotalTodos, 2);

    public decimal Progress => ProgressPercentage;

    public DateTimeOffset LastUpdatedAtUtc => Todos.Count == 0
        ? UpdatedAtUtc
        : Todos.Max(todo => todo.UpdatedAtUtc) > UpdatedAtUtc
            ? Todos.Max(todo => todo.UpdatedAtUtc)
            : UpdatedAtUtc;

    public Todo? NextTodo => Todos
        .Where(todo => !todo.Completed)
        .OrderBy(todo => todo.DueDate.HasValue ? 0 : 1)
        .ThenBy(todo => todo.DueDate)
        .ThenBy(todo => todo.CreatedAtUtc)
        .FirstOrDefault();

    public ProjectStatus Status
    {
        get
        {
            if (TotalTodos == 0)
            {
                return ProjectStatus.Planned;
            }

            return PendingTodos == 0
                ? ProjectStatus.Completed
                : ProjectStatus.InProgress;
        }
    }

    public override bool Completed
    {
        get => Status == ProjectStatus.Completed;
        protected set { }
    }

    public static Project Create(string name, string? description, string? color = null, DateOnly? expectedDate = null, ActivityAttribute? attribute = null)
    {
        var project = new Project();
        project.Update(name, description, color, expectedDate, attribute);
        return project;
    }

    public void Update(string name, string? description, string? color, DateOnly? expectedDate, ActivityAttribute? attribute = null)
    {
        UpdateDetails(name, description);
        Color = ProjectColor.Create(color).Value;
        ExpectedDate = expectedDate;
        SetAttribute(attribute);
    }

    public void SetArchived(bool archived)
    {
        Archived = archived;
        Touch();
    }

    public void AddTodo(Todo todo)
    {
        ArgumentNullException.ThrowIfNull(todo);

        if (Todos.Any(existingTodo => existingTodo.Id == todo.Id))
        {
            throw new InvalidDomainStateException($"To-Do '{todo.Id}' already belongs to project '{Id}'.");
        }

        if (UserId != Guid.Empty && todo.UserId != Guid.Empty && todo.UserId != UserId)
        {
            throw new InvalidDomainStateException("A Project cannot contain a To-Do owned by another User.");
        }

        if (UserId != Guid.Empty && todo.UserId == Guid.Empty)
        {
            todo.AssignOwner(UserId);
        }

        todo.AssignTo(Id);
        Todos.Add(todo);
        Touch();
    }

    public Todo FindTodo(Guid todoId) => Todos.FirstOrDefault(todo => todo.Id == todoId)
        ?? throw new InvalidDomainStateException($"To-Do '{todoId}' was not found in project '{Id}'.");

    public void RemoveTodo(Guid todoId)
    {
        Todos.Remove(FindTodo(todoId));
        Touch();
    }

    public override void ToggleCompletion() =>
        throw new InvalidDomainStateException("A Project cannot be completed manually. Complete its To-Dos instead.");
}
