using System.ComponentModel.DataAnnotations;
using BeeDay.Domain.Enums;

namespace BeeDay.Web.Components.Features.Projects.Models;

public sealed class ProjectEditorModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    // Projects no longer expose manual color selection — every project uses the
    // fixed --levelup-color-project Design System accent (kept as a plain string
    // here only because the Domain's ProjectColor value object still requires one).
    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a valid hexadecimal color.")]
    public string Color { get; set; } = ProjectAccentColor;

    public const string ProjectAccentColor = "#8056C7";

    public DateTime? ExpectedDate { get; set; }
    public bool Archived { get; set; }

    public ActivityAttribute? Attribute { get; set; }
}
