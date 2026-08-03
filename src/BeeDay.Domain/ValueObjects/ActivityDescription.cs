using BeeDay.Domain.Exceptions;

namespace BeeDay.Domain.ValueObjects;

public readonly record struct ActivityDescription
{
    public const int MaximumLength = 500;
    public string Value { get; }

    private ActivityDescription(string value) => Value = value;

    public static ActivityDescription Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length > MaximumLength)
        {
            throw new DomainValidationException("description", $"Description cannot exceed {MaximumLength} characters.");
        }
        return new ActivityDescription(normalized);
    }

    public override string ToString() => Value;
}
