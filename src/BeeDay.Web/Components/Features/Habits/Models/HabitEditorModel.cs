using System.ComponentModel.DataAnnotations;
using BeeDay.Domain.Enums;

namespace BeeDay.Web.Components.Features.Habits.Models;

public sealed class HabitEditorModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public BeeDay.Domain.Enums.HabitDirection Direction { get; set; } = BeeDay.Domain.Enums.HabitDirection.Both;
    public BeeDay.Domain.Enums.HabitDifficulty Difficulty { get; set; } = BeeDay.Domain.Enums.HabitDifficulty.Easy;
    public BeeDay.Domain.Enums.HabitResetCounter ResetCounter { get; set; } = BeeDay.Domain.Enums.HabitResetCounter.Daily;

    public ActivityAttribute? Attribute { get; set; }

    public int VisualBalance { get; set; }
}
