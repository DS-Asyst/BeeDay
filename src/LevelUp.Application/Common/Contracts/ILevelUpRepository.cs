using LevelUp.Domain.Entities;

namespace LevelUp.Application.Common.Contracts;

public interface ILevelUpRepository
{
    public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default);

    public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default);

    public Task UpdateAsync(
        Action<LevelUpData> mutation,
        CancellationToken cancellationToken = default);
}
