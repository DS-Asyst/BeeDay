using System.ComponentModel.DataAnnotations;
using LevelUp.Domain.Enums;

namespace LevelUp.Web.Components.Features.Habits.Models;

public sealed class HabitEditorModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public LevelUp.Domain.Enums.HabitDirection Direction { get; set; } = LevelUp.Domain.Enums.HabitDirection.Both;
    public LevelUp.Domain.Enums.HabitDifficulty Difficulty { get; set; } = LevelUp.Domain.Enums.HabitDifficulty.Easy;
    public LevelUp.Domain.Enums.HabitResetCounter ResetCounter { get; set; } = LevelUp.Domain.Enums.HabitResetCounter.Daily;

    public ActivityAttribute? Attribute { get; set; }

    public int VisualBalance { get; set; }
}
