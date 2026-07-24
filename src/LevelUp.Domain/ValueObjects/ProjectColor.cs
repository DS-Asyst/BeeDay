using System.Text.RegularExpressions;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.ValueObjects;

public readonly record struct ProjectColor
{
    public const string Default = "#7A4FCB";
    private static readonly Regex HexPattern = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    public string Value { get; }

    private ProjectColor(string value) => Value = value.ToUpperInvariant();

    public static ProjectColor Create(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? Default : value.Trim();
        if (!HexPattern.IsMatch(normalized))
            throw new DomainValidationException("Color", "Project color must use the #RRGGBB format.");
        return new ProjectColor(normalized);
    }

    public override string ToString() => Value;
}
