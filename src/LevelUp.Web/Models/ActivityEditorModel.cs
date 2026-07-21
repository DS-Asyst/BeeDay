using LevelUp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LevelUp.Web.Models;

public sealed class ActivityEditorModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public TaskRepeat Repeat { get; set; } = TaskRepeat.Daily;
    public DateTime? DueDate { get; set; }
    public ProjectStatus ProjectStatus { get; set; } = ProjectStatus.Planned;
}
