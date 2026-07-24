using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.ValueObjects;

public readonly record struct UserName
{
    public const int MaximumLength = 100;
    public string Value { get; }
    private UserName(string value) => Value = value;

    public static UserName Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new DomainValidationException("name", "Name is required.");
        if (normalized.Length > MaximumLength)
            throw new DomainValidationException("name", $"Name cannot exceed {MaximumLength} characters.");
        return new UserName(normalized);
    }

    public override string ToString() => Value;
}
