using LevelUp.Domain.Enums;

namespace LevelUp.Domain.Events;

public sealed record CharacterLeveledUpDomainEvent(
    Guid CharacterId,
    Guid ExperienceEntryId,
    int PreviousLevel,
    int NewLevel,
    int LevelsGained,
    long ExperienceAmount,
    ExperienceSourceType ExperienceSource,
    DateTimeOffset OccurredAtUtc) : DomainEvent;
