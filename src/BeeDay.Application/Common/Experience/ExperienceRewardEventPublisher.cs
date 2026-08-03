using BeeDay.Application.Common.Events;
using BeeDay.Domain.Events;
using BeeDay.Domain.Experience;
using MediatR;

namespace BeeDay.Application.Common.Experience;

public static class ExperienceRewardEventPublisher
{
    public static async Task PublishAsync(
        IPublisher publisher,
        Guid userId,
        ExperienceEntry? entry,
        CancellationToken cancellationToken)
    {
        if (entry?.Source.ReferenceId is not Guid sourceId)
        {
            return;
        }

        var experienceGrantedEvent = new ExperienceGrantedDomainEvent(
            userId,
            entry.Id,
            entry.Amount,
            entry.Source.Type,
            sourceId,
            entry.RewardType,
            entry.GrantedAtUtc);

        await publisher.Publish(
            new DomainEventNotification(experienceGrantedEvent),
            cancellationToken);

        if (entry.LevelAfter <= entry.LevelBefore)
        {
            return;
        }

        var userLeveledUpEvent = new UserLeveledUpDomainEvent(
            userId,
            entry.Id,
            entry.LevelBefore,
            entry.LevelAfter,
            entry.LevelAfter - entry.LevelBefore,
            entry.Amount,
            entry.Source.Type,
            entry.GrantedAtUtc)
        {
            OccurredOnUtc = entry.GrantedAtUtc,
        };

        await publisher.Publish(
            new DomainEventNotification(userLeveledUpEvent),
            cancellationToken);
    }
}
