using BeeDay.Domain.Enums;

namespace BeeDay.Application.Features.Projects.Requests;

public sealed record SaveProjectRequest(
    string Name,
    string Description,
    string Color,
    DateOnly? ExpectedDate,
    bool Archived,
    ActivityAttribute? Attribute = null);
