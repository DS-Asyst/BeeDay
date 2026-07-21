namespace LevelUp.Application.Features.Todos.Requests;

public sealed record SaveTodoRequest(
    string Title,
    string Description,
    DateOnly? DueDate);
