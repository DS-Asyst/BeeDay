using System.Text.Json.Serialization;
using LevelUp.Domain.Abstractions;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed class UserToken : Entity
{
    [JsonInclude] public Guid UserId { get; private set; }
    [JsonInclude] public UserTokenType Type { get; private set; }
    [JsonInclude] public string TokenHash { get; private set; } = string.Empty;
    [JsonInclude] public DateTimeOffset CreatedAtUtc { get; private set; }
    [JsonInclude] public DateTimeOffset ExpiresAtUtc { get; private set; }
    [JsonInclude] public DateTimeOffset? UsedAtUtc { get; private set; }
    [JsonInclude] public DateTimeOffset? RevokedAtUtc { get; private set; }

    [JsonIgnore] public bool IsUsed => UsedAtUtc.HasValue;
    [JsonIgnore] public bool IsRevoked => RevokedAtUtc.HasValue;

    public static UserToken Create(
        Guid userId,
        UserTokenType type,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException(nameof(userId), "A token must belong to a valid User.");
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainValidationException(nameof(type), $"Token type '{type}' is invalid.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new DomainValidationException(nameof(expiresAtUtc), "Token expiration must be later than its creation time.");
        }

        return new UserToken
        {
            UserId = userId,
            Type = type,
            TokenHash = tokenHash.Trim(),
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public bool IsExpired(DateTimeOffset nowUtc) => nowUtc >= ExpiresAtUtc;

    public void EnsureCanBeUsed(UserTokenType expectedType, DateTimeOffset nowUtc)
    {
        if (Type != expectedType)
        {
            throw new InvalidDomainStateException($"Token type '{Type}' cannot be used as '{expectedType}'.");
        }

        if (IsUsed)
        {
            throw new InvalidDomainStateException("Token has already been used.");
        }

        if (IsRevoked)
        {
            throw new InvalidDomainStateException("Token has been revoked.");
        }

        if (IsExpired(nowUtc))
        {
            throw new InvalidDomainStateException("Token has expired.");
        }
    }

    public void MarkAsUsed(UserTokenType expectedType, DateTimeOffset usedAtUtc)
    {
        EnsureCanBeUsed(expectedType, usedAtUtc);
        UsedAtUtc = usedAtUtc;
    }

    public void Revoke(DateTimeOffset revokedAtUtc)
    {
        if (IsUsed || IsRevoked)
        {
            return;
        }

        if (revokedAtUtc < CreatedAtUtc)
        {
            throw new DomainValidationException(nameof(revokedAtUtc), "Token revocation cannot precede its creation time.");
        }

        RevokedAtUtc = revokedAtUtc;
    }
}
