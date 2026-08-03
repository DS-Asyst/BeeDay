using BeeDay.Domain.Enums;

namespace BeeDay.Application.Features.Habits.Requests;

public sealed record SaveHabitRequest(
    string Title,
    string Description,
    HabitDirection Direction,
    HabitDifficulty Difficulty,
    HabitResetCounter ResetCounter,
    ActivityAttribute? Attribute = null);
