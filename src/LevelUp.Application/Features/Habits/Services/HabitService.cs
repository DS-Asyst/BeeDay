using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Services;
using LevelUp.Application.Features.Habits.Contracts;
using LevelUp.Application.Features.Habits.Requests;
using LevelUp.Domain.Entities;

namespace LevelUp.Application.Features.Habits.Services;

public sealed class HabitService(ILevelUpRepository repository)
    : ApplicationService(repository), IHabitService
{
    public Task AddAsync(SaveHabitRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync(data => data.AddHabit(Habit.Create(
            request.Title,
            request.Description,
            request.Direction,
            request.Difficulty,
            request.ResetCounter)), cancellationToken);

    public Task UpdateAsync(Guid id, SaveHabitRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync(data => Find(data.Habits, id).Update(
            request.Title,
            request.Description,
            request.Direction,
            request.Difficulty,
            request.ResetCounter), cancellationToken);

    public Task RegisterPositiveAsync(Guid id, CancellationToken cancellationToken = default) =>
        MutateAsync(data => Find(data.Habits, id).RegisterPositive(), cancellationToken);

    public Task RegisterNegativeAsync(Guid id, CancellationToken cancellationToken = default) =>
        MutateAsync(data => Find(data.Habits, id).RegisterNegative(), cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteAsync(data => data.Habits, id, cancellationToken);
}
