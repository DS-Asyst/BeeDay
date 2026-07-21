using LevelUp.Application.Features.Habits.Requests;

namespace LevelUp.Application.Features.Habits.Contracts;

public interface IHabitService
{
    public Task AddAsync(SaveHabitRequest request, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Guid id, SaveHabitRequest request, CancellationToken cancellationToken = default);
    public Task RegisterPositiveAsync(Guid id, CancellationToken cancellationToken = default);
    public Task RegisterNegativeAsync(Guid id, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
