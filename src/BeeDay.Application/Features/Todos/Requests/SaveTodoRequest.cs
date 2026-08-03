using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Todos.Requests;

public sealed record SaveTodoRequest(
    Guid ProjectId,
    string Title,
    string Description,
    DateOnly? DueDate,
    ActivityAttribute? Attribute = null);
