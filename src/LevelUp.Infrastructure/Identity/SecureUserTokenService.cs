using System.Security.Cryptography;
using System.Text;
using LevelUp.Application.Common.Identity;

namespace LevelUp.Infrastructure.Identity;

public sealed class SecureUserTokenService : IUserTokenService
{
    private const int TokenSizeInBytes = 32;

    public string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[TokenSizeInBytes];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
