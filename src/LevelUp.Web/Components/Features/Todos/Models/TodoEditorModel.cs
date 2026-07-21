using System.ComponentModel.DataAnnotations;

namespace LevelUp.Web.Components.Features.Todos.Models;

public sealed class TodoEditorModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }
}
