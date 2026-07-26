using LevelUp.Application.Common.Events;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Events;
using LevelUp.Domain.Experience;
using MediatR;

namespace LevelUp.Application.Common.Experience;

public static class ExperienceRewardEventPublisher
{
    public static async Task PublishAsync(
        IPublisher publisher,
        Guid userId,
        Character character,
        ExperienceEntry? entry,
        CancellationToken cancellationToken)
    {
        if (entry?.Source.ReferenceId is not Guid sourceId)
        {
            return;
        }

        var experienceGrantedEvent = new ExperienceGrantedDomainEvent(
            userId,
            character.Id,
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

        var characterLeveledUpEvent = new CharacterLeveledUpDomainEvent(
            character.Id,
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
            new DomainEventNotification(characterLeveledUpEvent),
            cancellationToken);
    }
}
