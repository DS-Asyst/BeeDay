using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Domain.Experience;
using Xunit;

namespace BeeDay.Domain.Tests;

public sealed class ExperienceDomainTests
{
    [Fact]
    public void New_user_starts_at_level_one_without_experience()
    {
        var user = CreateUser();

        Assert.Equal(0L, user.Experience.TotalExperience);
        Assert.Equal(1, user.Experience.CurrentLevel);
        Assert.Equal(0L, user.Experience.CurrentLevelExperience);
        Assert.Equal(100L, user.Experience.ExperienceRequiredForCurrentLevel);
        Assert.Equal(100L, user.Experience.ExperienceForNextLevel);
        Assert.Empty(user.Experience.Entries);
    }

    [Theory]
    [InlineData(0, 1, 0, 100)]
    [InlineData(99, 1, 99, 1)]
    [InlineData(100, 2, 0, 200)]
    [InlineData(299, 2, 199, 1)]
    [InlineData(300, 3, 0, 300)]
    [InlineData(600, 4, 0, 400)]
    public void Curve_derives_level_progress_and_remaining_experience(
        long totalExperience,
        int expectedLevel,
        long expectedCurrentLevelExperience,
        long expectedExperienceForNextLevel)
    {
        var experience = UserExperience.Create();
        if (totalExperience > 0)
        {
            experience.Add(
                ExperienceReward.Create(totalExperience),
                ExperienceSource.Create(ExperienceSourceType.System));
        }

        Assert.Equal(expectedLevel, experience.CurrentLevel);
        Assert.Equal(expectedCurrentLevelExperience, experience.CurrentLevelExperience);
        Assert.Equal(expectedExperienceForNextLevel, experience.ExperienceForNextLevel);
    }

    [Fact]
    public void Add_experience_updates_total_and_records_source_history()
    {
        var user = CreateUser();
        var sourceId = Guid.NewGuid();
        var occurredAtUtc = new DateTimeOffset(2026, 7, 26, 3, 30, 0, TimeSpan.Zero);
        var source = ExperienceSource.Create(ExperienceSourceType.Habit, sourceId, "Completed morning routine");

        var entry = user.AddExperience(ExperienceReward.Create(25), source, occurredAtUtc);

        Assert.Equal(25L, user.Experience.TotalExperience);
        Assert.Single(user.Experience.Entries);
        Assert.Same(entry, user.Experience.Entries[0]);
        Assert.Equal(25L, entry.Amount);
        Assert.Equal(ExperienceSourceType.Habit, entry.Source.Type);
        Assert.Equal(sourceId, entry.Source.ReferenceId);
        Assert.Equal("Completed morning routine", entry.Source.Description);
        Assert.Equal(occurredAtUtc, entry.OccurredAtUtc);
        Assert.Equal(occurredAtUtc, user.UpdatedAtUtc);
    }

    [Fact]
    public void Multiple_rewards_accumulate_without_storing_derived_level_state()
    {
        var experience = UserExperience.Create();

        experience.Add(
            ExperienceReward.Create(60),
            ExperienceSource.Create(ExperienceSourceType.Task, Guid.NewGuid()));
        experience.Add(
            ExperienceReward.Create(50),
            ExperienceSource.Create(ExperienceSourceType.Reading, Guid.NewGuid()));

        Assert.Equal(110L, experience.TotalExperience);
        Assert.Equal(2, experience.CurrentLevel);
        Assert.Equal(10L, experience.CurrentLevelExperience);
        Assert.Equal(190L, experience.ExperienceForNextLevel);
        Assert.Equal(2, experience.Entries.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Reward_rejects_non_positive_experience(long amount)
    {
        Assert.Throws<DomainValidationException>(() => ExperienceReward.Create(amount));
    }

    [Fact]
    public void Add_rejects_default_reward_that_bypasses_factory()
    {
        var experience = UserExperience.Create();

        Assert.Throws<DomainValidationException>(() => experience.Add(
            default,
            ExperienceSource.Create(ExperienceSourceType.System)));
    }

    [Fact]
    public void Source_rejects_empty_reference_identifier()
    {
        Assert.Throws<DomainValidationException>(() => ExperienceSource.Create(
            ExperienceSourceType.Project,
            Guid.Empty));
    }

    [Fact]
    public void Source_rejects_undefined_type()
    {
        Assert.Throws<DomainValidationException>(() => ExperienceSource.Create(
            (ExperienceSourceType)999));
    }

    [Fact]
    public void Source_normalizes_optional_description()
    {
        var source = ExperienceSource.Create(
            ExperienceSourceType.Manual,
            description: "  Bonus granted by administrator  ");

        Assert.Equal("Bonus granted by administrator", source.Description);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 100)]
    [InlineData(3, 300)]
    [InlineData(4, 600)]
    [InlineData(5, 1000)]
    public void Curve_calculates_cumulative_thresholds(int level, long expectedTotal)
    {
        Assert.Equal(expectedTotal, ExperienceCurve.GetTotalExperienceRequiredForLevel(level));
    }


    [Fact]
    public void Curve_contract_supports_alternative_linear_balance()
    {
        IExperienceCurve curve = new LinearExperienceCurve(baseExperience: 250);

        Assert.Equal(250L, curve.BaseExperience);
        Assert.Null(curve.MaximumLevel);
        Assert.Equal(2, curve.GetLevel(250));
        Assert.Equal(750L, curve.GetTotalExperienceRequiredForLevel(3));
        Assert.Equal(750L, curve.GetExperienceRequiredToAdvance(3));
    }

    [Fact]
    public void Curve_can_enforce_an_explicit_maximum_level()
    {
        IExperienceCurve curve = new LinearExperienceCurve(baseExperience: 100, maximumLevel: 5);

        Assert.Equal(5, curve.MaximumLevel);
        Assert.Equal(5, curve.GetLevel(long.MaxValue));
        Assert.Equal(0L, curve.GetExperienceRequiredToAdvance(5));
        Assert.Throws<DomainValidationException>(() => curve.GetTotalExperienceRequiredForLevel(6));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Curve_rejects_invalid_base_experience(long baseExperience)
    {
        Assert.Throws<DomainValidationException>(() => new LinearExperienceCurve(baseExperience));
    }

    [Fact]
    public void Curve_handles_the_largest_supported_experience_value()
    {
        var curve = ExperienceCurve.Default;

        var level = curve.GetLevel(long.MaxValue);
        var currentThreshold = curve.GetTotalExperienceRequiredForLevel(level);
        var advanceCost = curve.GetExperienceRequiredToAdvance(level);

        Assert.True(level > 1);
        Assert.True(currentThreshold <= long.MaxValue);
        Assert.True((decimal)currentThreshold + advanceCost > long.MaxValue);
    }

    [Fact]
    public void Curve_rejects_negative_total_experience()
    {
        Assert.Throws<DomainValidationException>(() => ExperienceCurve.GetLevel(-1));
    }

    [Fact]
    public void Curve_rejects_invalid_level()
    {
        Assert.Throws<DomainValidationException>(() => ExperienceCurve.GetTotalExperienceRequiredForLevel(0));
        Assert.Throws<DomainValidationException>(() => ExperienceCurve.GetExperienceRequiredToAdvance(0));
    }

    private static User CreateUser() =>
        User.Create("Hero", "hero@beeday.invalid");
}
