using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;

namespace LevelUp.Application.Common.Experience;

public interface IExperienceRewardService
{
    /// <summary>
    /// Grants XP directly on an already-loaded <paramref name="user"/> — the caller is responsible for
    /// loading the User and persisting it afterward (e.g. via <c>IUserRepository.UpdateAsync</c> or
    /// <c>IUnitOfWork</c>). Previously took <c>LevelUpData</c> and resolved the User internally — a
    /// violation of Contract-First flagged in docs/architecture/07-persistence-contracts.md §5.1,
    /// corrected here on the Sprint 14.6 cutover.
    /// </summary>
    public ExperienceEntry? Grant(
        User user,
        ExperienceSourceType sourceType,
        Guid sourceId,
        ExperienceRewardType rewardType,
        string? description = null,
        DateTimeOffset? grantedAtUtc = null);
}
