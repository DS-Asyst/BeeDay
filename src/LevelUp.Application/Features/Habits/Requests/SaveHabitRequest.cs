using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Habits.Requests;

public sealed record SaveHabitRequest(
    string Title,
    string Description,
    HabitDirection Direction,
    HabitDifficulty Difficulty,
    HabitResetCounter ResetCounter,
    ActivityAttribute? Attribute = null);
