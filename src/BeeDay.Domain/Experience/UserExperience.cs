using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Domain.Experience;

public sealed class UserExperience
{
    private UserExperience() { }

    public long TotalExperience { get; private set; }

    public IReadOnlyList<ExperienceEntry> Entries { get; private set; } = [];

    public int CurrentLevel => ExperienceCurve.GetLevel(TotalExperience);

    public long CurrentLevelExperience => TotalExperience - ExperienceCurve.GetTotalExperienceRequiredForLevel(CurrentLevel);

    public long ExperienceRequiredForCurrentLevel => ExperienceCurve.GetExperienceRequiredToAdvance(CurrentLevel);

    public long ExperienceForNextLevel => ExperienceRequiredForCurrentLevel - CurrentLevelExperience;

    public static UserExperience Create() => new();

    /// <summary>
    /// Materialization-only hook for Infrastructure: assigns previously-persisted entries without
    /// re-running the <c>Add</c> factory (which would double-count <see cref="TotalExperience"/> —
    /// that column is already loaded independently from its own row). Required because
    /// <see cref="Entries"/> is intentionally not an EF Core-mapped navigation (it would duplicate the
    /// real ExperienceEntry-to-User relationship); the caller is expected to load matching entries
    /// itself and hydrate this instance before granting further experience, so
    /// <see cref="TryAdd"/>'s duplicate check has real history to compare against instead of always
    /// seeing an empty collection.
    /// </summary>
    internal void Hydrate(IReadOnlyList<ExperienceEntry> entries) => Entries = entries;

    public ExperienceEntry Add(
        ExperienceReward reward,
        ExperienceSource source,
        DateTimeOffset? occurredAtUtc = null) =>
        Add(Guid.NewGuid(), reward, source, ExperienceRewardType.Completion, occurredAtUtc);

    public ExperienceEntry Add(
        Guid userId,
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType = ExperienceRewardType.Completion,
        DateTimeOffset? occurredAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (reward.Amount <= 0)
        {
            throw new DomainValidationException(nameof(reward), "Experience reward must be greater than zero.");
        }

        if (reward.Amount > long.MaxValue - TotalExperience)
        {
            throw new InvalidDomainStateException("Total experience exceeds the supported range.");
        }

        long experienceBefore = TotalExperience;
        int levelBefore = ExperienceCurve.GetLevel(experienceBefore);
        long experienceAfter = checked(experienceBefore + reward.Amount);
        int levelAfter = ExperienceCurve.GetLevel(experienceAfter);
        ExperienceEntry entry = ExperienceEntry.Create(
            userId,
            reward,
            source,
            rewardType,
            experienceBefore,
            experienceAfter,
            levelBefore,
            levelAfter,
            occurredAtUtc);

        TotalExperience = experienceAfter;
        Entries = [.. Entries, entry];

        return entry;
    }

    public ExperienceEntry? TryAdd(
        Guid userId,
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType,
        DateTimeOffset? grantedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.ReferenceId is not Guid sourceId)
        {
            throw new DomainValidationException(nameof(source), "Automatic experience rewards require a source identifier.");
        }

        bool duplicate = Entries.Any(entry =>
            entry.UserId == userId &&
            entry.Source.Type == source.Type &&
            entry.Source.ReferenceId == sourceId &&
            entry.RewardType == rewardType);

        return duplicate
            ? null
            : Add(userId, reward, source, rewardType, grantedAtUtc);
    }

    internal void EnsureValidState()
    {
        Entries ??= [];

        if (TotalExperience < 0)
        {
            throw new InvalidDomainStateException("Total experience cannot be negative.");
        }

        if (Entries.Any(entry => entry.Amount <= 0))
        {
            throw new InvalidDomainStateException("Experience history cannot contain non-positive rewards.");
        }

        bool duplicate = Entries
            .Where(entry => entry.Source.ReferenceId.HasValue && entry.Source.Type != ExperienceSourceType.Habit)
            .GroupBy(entry => new
            {
                entry.UserId,
                entry.Source.Type,
                entry.Source.ReferenceId,
                entry.RewardType,
            })
            .Any(group => group.Count() > 1);

        if (duplicate)
        {
            throw new InvalidDomainStateException("Experience history contains a duplicate reward key.");
        }
    }
}
