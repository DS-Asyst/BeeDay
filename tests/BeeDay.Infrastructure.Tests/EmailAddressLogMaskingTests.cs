using BeeDay.Infrastructure.Identity;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

public sealed class EmailAddressLogMaskingTests
{
    [Theory]
    [InlineData("tiago@beeday.example", "ti***@beeday.example")]
    [InlineData("a@beeday.example", "a***@beeday.example")]
    [InlineData("ab@beeday.example", "ab***@beeday.example")]
    public void Mask_KeepsAtMostTwoLocalPartCharactersAndTheFullDomain(string email, string expected)
    {
        Assert.Equal(expected, EmailAddressLogMasking.Mask(email));
    }

    [Fact]
    public void Mask_NeverReturnsTheOriginalAddressUnmodified()
    {
        const string email = "player@example.com";

        var masked = EmailAddressLogMasking.Mask(email);

        Assert.NotEqual(email, masked);
        Assert.DoesNotContain("player", masked, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Mask_HandlesMissingAtSignWithoutThrowing(string value)
    {
        Assert.Equal("***", EmailAddressLogMasking.Mask(value));
    }
}
