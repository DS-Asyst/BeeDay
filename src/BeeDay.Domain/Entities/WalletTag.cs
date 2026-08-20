using System.Text.RegularExpressions;
using BeeDay.Domain.Abstractions;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Domain.Entities;

public sealed partial class WalletTag : Entity
{
    private WalletTag() { }

    public const int MaximumNameLength = 40;
    public const string DefaultColor = "#7A4FCB";

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Color { get; private set; } = DefaultColor;

    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public static WalletTag Create(Guid userId, string name, string? color = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException(nameof(userId), "User identifier is required.");
        }

        var tag = new WalletTag { UserId = userId };
        tag.Update(name, color);
        return tag;
    }

    public void Rename(string name)
    {
        Name = NormalizeName(name);
        Touch();
    }

    public void ChangeColor(string? color)
    {
        Color = NormalizeColor(color);
        Touch();
    }

    public void Update(string name, string? color)
    {
        Name = NormalizeName(name);
        Color = NormalizeColor(color);
        Touch();
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException(nameof(name), "Tag name is required.");
        }

        var normalized = string.Join(' ', name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length > MaximumNameLength)
        {
            throw new DomainValidationException(nameof(name), $"Tag name cannot exceed {MaximumNameLength} characters.");
        }

        return normalized;
    }

    private static string NormalizeColor(string? color)
    {
        var normalized = string.IsNullOrWhiteSpace(color) ? DefaultColor : color.Trim().ToUpperInvariant();
        if (!HexColorRegex().IsMatch(normalized))
        {
            throw new DomainValidationException(nameof(color), "Tag color must use the #RRGGBB hexadecimal format.");
        }

        return normalized;
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;

    [GeneratedRegex("^#[0-9A-F]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexColorRegex();
}
