using BeeDay.Domain.Enums;

namespace BeeDay.Domain.Events;

public sealed record ExperienceGrantedDomainEvent(
    Guid UserId,
    Guid TransactionId,
    long Amount,
    ExperienceSourceType SourceType,
    Guid SourceId,
    ExperienceRewardType RewardType,
    DateTimeOffset GrantedAtUtc) : DomainEvent;
