using BeeDay.Application.Common.Events;
using BeeDay.Domain.Events;
using MediatR;

namespace BeeDay.Web.Components.Features.Experience.Feedback;

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
