using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Experience;

public sealed class ExperienceSource
{
    public const int MaximumDescriptionLength = 160;

    public ExperienceSourceType Type { get; private set; }

    public Guid? ReferenceId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public static ExperienceSource Create(
        ExperienceSourceType type,
        Guid? referenceId = null,
        string? description = null)
    {
        if (referenceId == Guid.Empty)
        {
            throw new DomainValidationException(nameof(referenceId), "Experience source reference cannot be empty.");
        }

        var normalizedDescription = description?.Trim() ?? string.Empty;
        if (normalizedDescription.Length > MaximumDescriptionLength)
        {
            throw new DomainValidationException(
                nameof(description),
                $"Experience source description cannot exceed {MaximumDescriptionLength} characters.");
        }

        return new ExperienceSource
        {
            Type = EnumValidation.Defined(type, nameof(type)),
            ReferenceId = referenceId,
            Description = normalizedDescription,
        };
    }
}
