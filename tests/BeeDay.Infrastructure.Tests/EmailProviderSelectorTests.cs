using BeeDay.Infrastructure.Configuration;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

public sealed class EmailProviderSelectorTests
{
    [Theory]
    [InlineData(true, false, EmailProvider.Resend)]
    [InlineData(false, true, EmailProvider.Development)]
    public void Resolve_WithExactlyOneProviderEnabled_ReturnsThatProvider(bool resendEnabled, bool developmentEnabled, EmailProvider expected)
    {
        var result = EmailProviderSelector.Resolve(resendEnabled, developmentEnabled);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Resolve_WhenBothProvidersEnabled_ThrowsAmbiguousConfiguration()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => EmailProviderSelector.Resolve(resendEnabled: true, developmentEnabled: true));

        Assert.Contains("Ambiguous", exception.Message, StringComparison.Ordinal);
        Assert.Contains(ResendOptions.SectionName, exception.Message, StringComparison.Ordinal);
        Assert.Contains(DevelopmentEmailOptions.SectionName, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_WhenNoProviderEnabled_ThrowsInvalidConfiguration()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => EmailProviderSelector.Resolve(resendEnabled: false, developmentEnabled: false));

        Assert.Contains("Invalid", exception.Message, StringComparison.Ordinal);
    }
}
