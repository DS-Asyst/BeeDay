using System.Text.Json.Serialization;
using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;

namespace LevelUp.Domain.Entities;

public sealed class Project : Activity
{
    [JsonInclude]
    public ProjectStatus Status { get; private set; } = ProjectStatus.Planned;

    public static Project Create(string title, string? description, ProjectStatus status)
    {
        var project = new Project();
        project.Update(title, description, status);
        return project;
    }

    public void Update(string title, string? description, ProjectStatus status)
    {
        UpdateDetails(title, description);
        SetStatus(status);
    }

    public void SetStatus(ProjectStatus status)
    {
        Status = EnumValidation.Defined(status, nameof(status));
        Completed = Status == ProjectStatus.Completed;
        Touch();
    }

    public void ToggleStatus() => SetStatus(Completed ? ProjectStatus.InProgress : ProjectStatus.Completed);

    public override void ToggleCompletion() => ToggleStatus();
}
