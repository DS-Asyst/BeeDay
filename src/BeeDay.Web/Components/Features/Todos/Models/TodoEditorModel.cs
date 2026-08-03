using System.ComponentModel.DataAnnotations;
using BeeDay.Domain.Enums;

namespace BeeDay.Web.Components.Features.Todos.Models;

public sealed class TodoEditorModel
{
    [Required(ErrorMessage = "Project is required.")]
    public Guid? ProjectId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public ActivityAttribute? Attribute { get; set; }
}
