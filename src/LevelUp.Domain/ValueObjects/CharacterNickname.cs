using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.ValueObjects;

public readonly record struct CharacterNickname
{
    public const int MinimumLength = 3;
    public const int MaximumLength = 20;
    public string Value { get; }
    private CharacterNickname(string value) => Value = value;

    public static CharacterNickname Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().TrimStart('@');
        if (normalized.Length is < MinimumLength or > MaximumLength)
        {
            throw new DomainValidationException("nickname", $"Nickname must contain between {MinimumLength} and {MaximumLength} characters.");
        }
        if (!normalized.All(character => char.IsLetterOrDigit(character) || character is '.' or '_' or '-'))
        {
            throw new DomainValidationException("nickname", "Nickname may contain only letters, numbers, dots, underscores and hyphens.");
        }
        return new CharacterNickname(normalized);
    }

    public override string ToString() => Value;
}
