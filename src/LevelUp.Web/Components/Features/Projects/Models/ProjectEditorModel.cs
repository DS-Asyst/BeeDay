using System.ComponentModel.DataAnnotations;

namespace LevelUp.Web.Components.Features.Projects.Models;

public sealed class ProjectEditorModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a valid hexadecimal color.")]
    public string Color { get; set; } = "#7A4FCB";

    public DateTime? ExpectedDate { get; set; }
    public bool Archived { get; set; }
}
