using System.Text.Json.Serialization;
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

        var transaction = ExperienceTransaction.Create(reward, source, occurredAtUtc);
        TotalExperience += reward.Amount;
        Transactions = [.. Transactions, transaction];
        return transaction;
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
    }
}
