using BeeDay.Domain.Enums;
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

    [Theory]
    [InlineData(UserLanguage.English, "en-US")]
    [InlineData(UserLanguage.Portuguese, "pt-BR")]
    public void FromUserLanguage_MapsToTheMatchingSupportedCulture(UserLanguage language, string expectedCulture)
    {
        Assert.Equal(expectedCulture, BeeDayCultures.FromUserLanguage(language));
    }

    [Theory]
    [InlineData("en-US", UserLanguage.English)]
    [InlineData("pt-BR", UserLanguage.Portuguese)]
    public void ToUserLanguage_MapsToTheMatchingDomainEnum(string culture, UserLanguage expectedLanguage)
    {
        Assert.Equal(expectedLanguage, BeeDayCultures.ToUserLanguage(culture));
    }

    [Fact]
    public void ToUserLanguage_WithAnUnsupportedCulture_FallsBackToEnglish()
    {
        Assert.Equal(UserLanguage.English, BeeDayCultures.ToUserLanguage("fr-FR"));
    }

    [Fact]
    public void RoundTrip_FromUserLanguageThenToUserLanguage_IsLossless()
    {
        foreach (var language in new[] { UserLanguage.English, UserLanguage.Portuguese })
        {
            Assert.Equal(language, BeeDayCultures.ToUserLanguage(BeeDayCultures.FromUserLanguage(language)));
        }
    }

    [Fact]
    public void CreateCookieOptions_InDevelopment_IsNotForcedSecure()
    {
        var options = BeeDayCultures.CreateCookieOptions(isDevelopment: true);

        Assert.True(options.HttpOnly);
        Assert.Equal(Microsoft.AspNetCore.Http.SameSiteMode.Lax, options.SameSite);
        Assert.False(options.Secure);
    }

    [Fact]
    public void CreateCookieOptions_OutsideDevelopment_IsAlwaysSecure()
    {
        var options = BeeDayCultures.CreateCookieOptions(isDevelopment: false);

        Assert.True(options.Secure);
    }
}
