using System.ComponentModel.DataAnnotations;
using BeeDay.Domain.Enums;

namespace BeeDay.Web.Components.Features.Tasks.Models;

public sealed class TaskEditorModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public BeeDay.Domain.Enums.TaskRepeat Repeat { get; set; } = BeeDay.Domain.Enums.TaskRepeat.Daily;

    public ActivityAttribute? Attribute { get; set; }
}
