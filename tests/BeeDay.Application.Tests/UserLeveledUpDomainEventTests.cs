using BeeDay.Application.Common.Events;
using BeeDay.Application.Common.Experience;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Events;
using BeeDay.Domain.Experience;
using MediatR;

namespace BeeDay.Application.Tests;

public sealed class UserLeveledUpDomainEventTests
{
    [Fact]
    public async Task Reward_without_level_change_does_not_publish_level_up_event()
    {
        User user = CreateUser();
        ExperienceEntry entry = user.AddExperience(
            ExperienceReward.Create(5),
            ExperienceSource.Create(ExperienceSourceType.Task, Guid.NewGuid()));
        var publisher = new CapturingPublisher();

        await ExperienceRewardEventPublisher.PublishAsync(
            publisher,
            user.Id,
            entry,
            TestContext.Current.CancellationToken);

        Assert.Single(publisher.Notifications);
        Assert.IsType<ExperienceGrantedDomainEvent>(publisher.Notifications[0].DomainEvent);
    }

    [Fact]
    public async Task Reward_crossing_level_boundary_publishes_one_level_up_event()
    {
        User user = CreateUser();
        user.AddExperience(
            ExperienceReward.Create(95),
            ExperienceSource.Create(ExperienceSourceType.System));
        ExperienceEntry entry = user.AddExperience(
            ExperienceReward.Create(5),
            ExperienceSource.Create(ExperienceSourceType.Task, Guid.NewGuid()));
        var publisher = new CapturingPublisher();

        await ExperienceRewardEventPublisher.PublishAsync(
            publisher,
            user.Id,
            entry,
            TestContext.Current.CancellationToken);

        UserLeveledUpDomainEvent domainEvent = Assert.Single(
            publisher.Notifications.Select(item => item.DomainEvent).OfType<UserLeveledUpDomainEvent>());
        Assert.Equal(user.Id, domainEvent.UserId);
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
        User user = CreateUser();
        ExperienceEntry entry = user.AddExperience(
            ExperienceReward.Create(1000),
            ExperienceSource.Create(ExperienceSourceType.Project, Guid.NewGuid()));
        var publisher = new CapturingPublisher();

        await ExperienceRewardEventPublisher.PublishAsync(
            publisher,
            user.Id,
            entry,
            TestContext.Current.CancellationToken);

        UserLeveledUpDomainEvent domainEvent = Assert.Single(
            publisher.Notifications.Select(item => item.DomainEvent).OfType<UserLeveledUpDomainEvent>());
        Assert.Equal(entry.LevelBefore, domainEvent.PreviousLevel);
        Assert.Equal(entry.LevelAfter, domainEvent.NewLevel);
        Assert.Equal(entry.LevelAfter - entry.LevelBefore, domainEvent.LevelsGained);
        Assert.True(domainEvent.LevelsGained > 1);
    }

    private static User CreateUser() =>
        User.Create("Event Hero", "eventhero@beeday.invalid");

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
