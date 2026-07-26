using LevelUp.Application.Common.Events;
using LevelUp.Application.Common.Experience;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Events;
using LevelUp.Domain.Experience;
using MediatR;

namespace LevelUp.Application.Tests;

public sealed class LevelUpEventTests
{
    [Fact]
    public async Task Reward_without_level_change_does_not_publish_level_up_event()
    {
        Character character = CreateCharacter();
        ExperienceEntry entry = character.AddExperience(
            ExperienceReward.Create(5),
            ExperienceSource.Create(ExperienceSourceType.Task, Guid.NewGuid()));
        var publisher = new CapturingPublisher();

        await ExperienceRewardEventPublisher.PublishAsync(
            publisher,
            character.UserId,
            character,
            entry,
            TestContext.Current.CancellationToken);

        Assert.Single(publisher.Notifications);
        Assert.IsType<ExperienceGrantedDomainEvent>(publisher.Notifications[0].DomainEvent);
    }

    [Fact]
    public async Task Reward_crossing_level_boundary_publishes_one_level_up_event()
    {
        Character character = CreateCharacter();
        character.AddExperience(
            ExperienceReward.Create(95),
            ExperienceSource.Create(ExperienceSourceType.System));
        ExperienceEntry entry = character.AddExperience(
            ExperienceReward.Create(5),
            ExperienceSource.Create(ExperienceSourceType.Task, Guid.NewGuid()));
        var publisher = new CapturingPublisher();

        await ExperienceRewardEventPublisher.PublishAsync(
            publisher,
            character.UserId,
            character,
            entry,
            TestContext.Current.CancellationToken);

        CharacterLeveledUpDomainEvent domainEvent = Assert.Single(
            publisher.Notifications.Select(item => item.DomainEvent).OfType<CharacterLeveledUpDomainEvent>());
        Assert.Equal(character.Id, domainEvent.CharacterId);
        Assert.Equal(entry.Id, domainEvent.ExperienceEntryId);
        Assert.Equal(1, domainEvent.PreviousLevel);
        Assert.Equal(2, domainEvent.NewLevel);
        Assert.Equal(1, domainEvent.LevelsGained);
        Assert.Equal(5, domainEvent.ExperienceAmount);
        Assert.Equal(ExperienceSourceType.Task, domainEvent.ExperienceSource);
        Assert.Equal(entry.GrantedAtUtc, domainEvent.OccurredAtUtc);
    }

    [Fact]
    public async Task Reward_crossing_multiple_levels_publishes_single_aggregate_event()
    {
        Character character = CreateCharacter();
        ExperienceEntry entry = character.AddExperience(
            ExperienceReward.Create(1000),
            ExperienceSource.Create(ExperienceSourceType.Project, Guid.NewGuid()));
        var publisher = new CapturingPublisher();

        await ExperienceRewardEventPublisher.PublishAsync(
            publisher,
            character.UserId,
            character,
            entry,
            TestContext.Current.CancellationToken);

        CharacterLeveledUpDomainEvent domainEvent = Assert.Single(
            publisher.Notifications.Select(item => item.DomainEvent).OfType<CharacterLeveledUpDomainEvent>());
        Assert.Equal(entry.LevelBefore, domainEvent.PreviousLevel);
        Assert.Equal(entry.LevelAfter, domainEvent.NewLevel);
        Assert.Equal(entry.LevelAfter - entry.LevelBefore, domainEvent.LevelsGained);
        Assert.True(domainEvent.LevelsGained > 1);
    }

    private static Character CreateCharacter() =>
        Character.Create(Guid.NewGuid(), "eventhero", CharacterClass.Warrior);

    private sealed class CapturingPublisher : IPublisher
    {
        public List<DomainEventNotification> Notifications { get; } = [];

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            Notifications.Add(Assert.IsType<DomainEventNotification>(notification));
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            Notifications.Add(Assert.IsType<DomainEventNotification>(notification));
            return Task.CompletedTask;
        }
    }
}
