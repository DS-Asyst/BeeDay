using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Web.Localization;
using Microsoft.AspNetCore.Http;

namespace BeeDay.Web.Tests.Localization;

/// <summary>
/// Unit-level coverage for AuthenticatedAccountCultureProvider in isolation, complementing the
/// end-to-end proof in AuthenticatedCultureIntegrationTests (which exercises it through the real
/// RequestLocalizationMiddleware pipeline via a forged persistent "BeeDay.Auth" cookie).
/// </summary>
public sealed class AuthenticatedAccountCultureProviderTests
{
    [Fact]
    public async Task WithNoUserStashedOnHttpContextItems_ReturnsNull()
    {
        var provider = new AuthenticatedAccountCultureProvider(isDevelopment: true);
        var httpContext = new DefaultHttpContext();

        var result = await provider.DetermineProviderCultureResult(httpContext);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(UserLanguage.Portuguese, "pt-BR")]
    [InlineData(UserLanguage.English, "en-US")]
    public async Task WithAUserStashed_ResolvesTheAccountCultureAndWritesTheCookie(UserLanguage language, string expectedCulture)
    {
        var provider = new AuthenticatedAccountCultureProvider(isDevelopment: true);
        var httpContext = new DefaultHttpContext();
        var user = User.Create("Test User", "test@beeday.invalid");
        user.UpdatePreferences(language, UserTheme.System);
        httpContext.Items[AuthenticatedAccountCultureProvider.HttpContextItemsKey] = user;

        var result = await provider.DetermineProviderCultureResult(httpContext);

        Assert.NotNull(result);
        Assert.Equal(expectedCulture, result!.Cultures[0].Value);

        Assert.True(httpContext.Response.Headers.TryGetValue("Set-Cookie", out var setCookie));
        Assert.Contains($"{BeeDayCultures.CookieName}=", setCookie.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithSomethingOtherThanAUserStashed_ReturnsNull()
    {
        var provider = new AuthenticatedAccountCultureProvider(isDevelopment: true);
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthenticatedAccountCultureProvider.HttpContextItemsKey] = "not a user";

        var result = await provider.DetermineProviderCultureResult(httpContext);

        Assert.Null(result);
    }
}
