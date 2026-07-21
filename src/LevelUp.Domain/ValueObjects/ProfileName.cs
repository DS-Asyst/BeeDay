using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.ValueObjects;

public readonly record struct ProfileName
{
    public const int MaximumLength = 100;
    public string Value { get; }
    private ProfileName(string value) => Value = value;

    public static ProfileName Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainValidationException("name", "Name is required.");
        }
        if (normalized.Length > MaximumLength)
        {
            throw new DomainValidationException("name", $"Name cannot exceed {MaximumLength} characters.");
        }
        return new ProfileName(normalized);
    }

    public override string ToString() => Value;
}
