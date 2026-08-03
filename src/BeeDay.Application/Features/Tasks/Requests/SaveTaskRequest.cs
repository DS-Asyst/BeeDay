using BeeDay.Domain.Enums;

namespace BeeDay.Application.Features.Tasks.Requests;

public sealed record SaveTaskRequest(
    string Title,
    string Description,
    TaskRepeat Repeat,
    ActivityAttribute? Attribute = null);
