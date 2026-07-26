using System.Text.Json.Serialization;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Experience;

public sealed class CharacterExperience
{
    [JsonInclude]
    public long TotalExperience { get; private set; }

    [JsonInclude]
    public IReadOnlyList<ExperienceTransaction> Transactions { get; private set; } = [];

    [JsonIgnore]
    public int CurrentLevel => ExperienceCurve.GetLevel(TotalExperience);

    [JsonIgnore]
    public long CurrentLevelExperience =>
        TotalExperience - ExperienceCurve.GetTotalExperienceRequiredForLevel(CurrentLevel);

    [JsonIgnore]
    public long ExperienceRequiredForCurrentLevel =>
        ExperienceCurve.GetExperienceRequiredToAdvance(CurrentLevel);

    [JsonIgnore]
    public long ExperienceForNextLevel =>
        ExperienceRequiredForCurrentLevel - CurrentLevelExperience;

    public static CharacterExperience Create() => new();

    public ExperienceTransaction Add(
        ExperienceReward reward,
        ExperienceSource source,
        DateTimeOffset? occurredAtUtc = null) =>
        Add(reward, source, ExperienceRewardType.Completion, occurredAtUtc);

    public ExperienceTransaction Add(
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType,
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

        var transaction = ExperienceTransaction.Create(reward, source, rewardType, occurredAtUtc);
        TotalExperience += reward.Amount;
        Transactions = [.. Transactions, transaction];
        return transaction;
    }


    public ExperienceTransaction? TryAdd(
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

        var alreadyGranted = Transactions.Any(transaction =>
            transaction.Source.Type == source.Type
            && transaction.Source.ReferenceId == sourceId
            && transaction.RewardType == rewardType);

        return alreadyGranted ? null : Add(reward, source, rewardType, grantedAtUtc);
    }

    internal void EnsureValidState()
    {
        Transactions ??= [];

        if (TotalExperience < 0)
        {
            throw new InvalidDomainStateException("Character total experience cannot be negative.");
        }

        if (Transactions.Any(transaction => transaction.Amount <= 0))
        {
            throw new InvalidDomainStateException("Experience history cannot contain non-positive rewards.");
        }

        var duplicateRewardExists = Transactions
            .Where(transaction => transaction.Source.ReferenceId.HasValue)
            .GroupBy(transaction => new
            {
                transaction.Source.Type,
                transaction.Source.ReferenceId,
                transaction.RewardType
            })
            .Any(group => group.Count() > 1);

        if (duplicateRewardExists)
        {
            throw new InvalidDomainStateException("Experience history contains a duplicate reward key.");
        }
    }
}
