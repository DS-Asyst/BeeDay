using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.ValueObjects;

public readonly record struct ProfileNickname
{
    public const int MaximumLength = 100;
    public string Value { get; }
    private ProfileNickname(string value) => Value = value;

    public static ProfileNickname Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().TrimStart('@');
        if (normalized.Length > MaximumLength)
        {
            throw new DomainValidationException("nickname", $"Nickname cannot exceed {MaximumLength} characters.");
        }
        return new ProfileNickname(normalized);
    }

    public override string ToString() => Value;
}
