using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Tasks.Requests;

public sealed record SaveTaskRequest(
    string Title,
    string Description,
    TaskRepeat Repeat,
    ActivityAttribute? Attribute = null);
