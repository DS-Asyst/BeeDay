using System.Text.Json.Serialization;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Domain.Entities;

public sealed class Project : Activity
{
    [JsonInclude]
    public string Color { get; private set; } = ProjectColor.Default;

    [JsonInclude]
    public DateOnly? ExpectedDate { get; private set; }

    [JsonInclude]
    public bool Archived { get; private set; }

    [JsonInclude]
    public List<Todo> Todos { get; private set; } = [];

    [JsonIgnore]
    public string Name => Title;

    [JsonIgnore]
    public int TotalTodos => Todos.Count;

    [JsonIgnore]
    public int PendingTodos => Todos.Count(todo => !todo.Completed);

    [JsonIgnore]
    public int CompletedTodos => Todos.Count(todo => todo.Completed);

    [JsonIgnore]
    public decimal ProgressPercentage => TotalTodos == 0
        ? 0m
        : decimal.Round(CompletedTodos * 100m / TotalTodos, 2);

    [JsonIgnore]
    public decimal Progress => ProgressPercentage;

    [JsonIgnore]
    public DateTimeOffset LastUpdatedAtUtc => Todos.Count == 0
        ? UpdatedAtUtc
        : Todos.Max(todo => todo.UpdatedAtUtc) > UpdatedAtUtc
            ? Todos.Max(todo => todo.UpdatedAtUtc)
            : UpdatedAtUtc;

    [JsonIgnore]
    public Todo? NextTodo => Todos
        .Where(todo => !todo.Completed)
        .OrderBy(todo => todo.DueDate.HasValue ? 0 : 1)
        .ThenBy(todo => todo.DueDate)
        .ThenBy(todo => todo.CreatedAtUtc)
        .FirstOrDefault();

    [JsonIgnore]
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

    [JsonIgnore]
    public override bool Completed
    {
        get => Status == ProjectStatus.Completed;
        protected set { }
    }

    public static Project Create(string name, string? description, string? color = null, DateOnly? expectedDate = null)
    {
        var project = new Project();
        project.Update(name, description, color, expectedDate);
        return project;
    }

    public void Update(string name, string? description, string? color, DateOnly? expectedDate)
    {
        UpdateDetails(name, description);
        Color = ProjectColor.Create(color).Value;
        ExpectedDate = expectedDate;
    }

    public void SetArchived(bool archived)
    {
        Archived = archived;
        Touch();
    }

    public void AddTodo(Todo todo)
    {
        ArgumentNullException.ThrowIfNull(todo);
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
