using LevelUp.Application.Common.Events;
using LevelUp.Domain.Events;
using MediatR;

namespace LevelUp.Web.Components.Features.Experience.Feedback;

public sealed class LevelUpFeedbackEventHandler(LevelUpFeedbackStore store)
    : INotificationHandler<DomainEventNotification>
{
    public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        if (notification.DomainEvent is not UserLeveledUpDomainEvent domainEvent)
        {
            return Task.CompletedTask;
        }

        store.Add(new LevelUpFeedback(
            domainEvent.EventId,
            domainEvent.ExperienceEntryId,
            domainEvent.PreviousLevel,
            domainEvent.NewLevel,
            domainEvent.LevelsGained,
            domainEvent.ExperienceAmount,
            domainEvent.ExperienceSource,
            domainEvent.OccurredAtUtc));

        return Task.CompletedTask;
    }
}
