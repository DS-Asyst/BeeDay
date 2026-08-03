using BeeDay.Domain.Enums;

namespace BeeDay.Application.Features.Todos.Requests;

public sealed record SaveTodoRequest(
    Guid ProjectId,
    string Title,
    string Description,
    DateOnly? DueDate,
    ActivityAttribute? Attribute = null);
