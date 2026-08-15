using BeeDay.Web.Localization;

namespace BeeDay.Web.Tests.Localization;

public sealed class BeeDayCulturesTests
{
    [Fact]
    public void Supported_ContainsExactlyEnglishAndPortuguese()
    {
        Assert.Equal(["en-US", "pt-BR"], BeeDayCultures.Supported);
    }

    [Fact]
    public void Default_IsEnglish()
    {
        Assert.Equal("en-US", BeeDayCultures.Default);
        Assert.Contains(BeeDayCultures.Default, BeeDayCultures.Supported);
    }

    [Fact]
    public void CookieName_IsDistinctFromTheAuthenticationCookie()
    {
        Assert.Equal("BeeDay.Culture", BeeDayCultures.CookieName);
        Assert.NotEqual("BeeDay.Auth", BeeDayCultures.CookieName);
    }
}
