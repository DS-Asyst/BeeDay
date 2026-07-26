using LevelUp.Application.Common.Events;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Events;
using LevelUp.Domain.Experience;
using MediatR;

namespace LevelUp.Application.Common.Experience;

public static class ExperienceRewardEventPublisher
{
    public static Task PublishAsync(
        IPublisher publisher,
        Guid userId,
        Character character,
        ExperienceTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (transaction?.Source.ReferenceId is not Guid sourceId)
        {
            return Task.CompletedTask;
        }

        var domainEvent = new ExperienceGrantedDomainEvent(
            userId,
            character.Id,
            transaction.Id,
            transaction.Amount,
            transaction.Source.Type,
            sourceId,
            transaction.RewardType,
            transaction.GrantedAtUtc);

        return publisher.Publish(new DomainEventNotification(domainEvent), cancellationToken);
    }
}
