using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Projects.Requests;

public sealed record SaveProjectRequest(
    string Title,
    string Description,
    ProjectStatus Status);
