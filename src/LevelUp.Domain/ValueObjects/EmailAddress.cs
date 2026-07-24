using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.ValueObjects;

public readonly record struct EmailAddress
{
    public const int MaximumLength = 254;
    public string Value { get; }
    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new DomainValidationException("email", "Email is required.");
        if (normalized.Length > MaximumLength || !System.Net.Mail.MailAddress.TryCreate(normalized, out _))
            throw new DomainValidationException("email", "Email is invalid.");
        return new EmailAddress(normalized);
    }

    public override string ToString() => Value;
}
