using LevelUp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LevelUp.Web.Models;

public sealed class HabitEditorModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public HabitDirection Direction { get; set; } = HabitDirection.Both;
    public HabitDifficulty Difficulty { get; set; } = HabitDifficulty.Easy;
    public HabitResetCounter ResetCounter { get; set; } = HabitResetCounter.Daily;
}
