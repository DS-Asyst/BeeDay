using BeeDay.Domain.Exceptions;

namespace BeeDay.Domain.ValueObjects;

public readonly record struct ActivityTitle
{
    public const int MaximumLength = 100;
    public string Value { get; }

    private ActivityTitle(string value) => Value = value;

    public static ActivityTitle Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainValidationException("title", "Title is required.");
        }
        if (normalized.Length > MaximumLength)
        {
            throw new DomainValidationException("title", $"Title cannot exceed {MaximumLength} characters.");
        }
        return new ActivityTitle(normalized);
    }

    public override string ToString() => Value;
}
