using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Experience;

public readonly record struct ExperienceReward
{
    private ExperienceReward(long amount) => Amount = amount;

    public long Amount { get; }

    public static ExperienceReward Create(long amount)
    {
        if (amount <= 0)
        {
            throw new DomainValidationException(nameof(amount), "Experience reward must be greater than zero.");
        }

        return new ExperienceReward(amount);
    }
}
