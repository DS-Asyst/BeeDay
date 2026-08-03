using BeeDay.Infrastructure.Security;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

public sealed class Pbkdf2PasswordServiceTests
{
    private readonly Pbkdf2PasswordService _service = new();

    [Fact]
    public void Hash_ProducesSupportedFormatAndVerifiesPassword()
    {
        const string password = "LevelUp123";

        var hash = _service.Hash(password);

        Assert.StartsWith("PBKDF2-SHA256$120000$", hash, StringComparison.Ordinal);
        Assert.True(_service.Verify(password, hash));
    }

    [Fact]
    public void Hash_UsesRandomSaltForSamePassword()
    {
        const string password = "LevelUp123";

        var first = _service.Hash(password);
        var second = _service.Hash(password);

        Assert.NotEqual(first, second);
        Assert.True(_service.Verify(password, first));
        Assert.True(_service.Verify(password, second));
    }

    [Theory]
    [InlineData("WrongPassword1")]
    [InlineData("")]
    public void Verify_RejectsIncorrectPassword(string password)
    {
        var hash = _service.Hash("LevelUp123");

        Assert.False(_service.Verify(password, hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("PBKDF2-SHA256$0$bad$bad")]
    [InlineData("PBKDF2-SHA256$120000$not-base64$not-base64")]
    [InlineData("OTHER$120000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    public void Verify_RejectsMalformedOrUnsupportedHash(string hash)
    {
        Assert.False(_service.Verify("LevelUp123", hash));
    }
}
