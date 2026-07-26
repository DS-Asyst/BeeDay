using LevelUp.Web.Services.Authentication;

namespace LevelUp.Web.Tests.Services;

public sealed class LoginDestinationResolverTests
{
    [Fact]
    public void Resolve_WithoutCharacter_RequiresCharacterCreation()
    {
        var destination = LoginDestinationResolver.Resolve(
            hasCharacter: false,
            hasCompletedOnboarding: false,
            returnUrl: "/daily");

        Assert.Equal("/character/create", destination);
    }

    [Fact]
    public void Resolve_FirstLoginWithCharacter_RequiresTutorialEvenWhenReturnUrlTargetsDaily()
    {
        var destination = LoginDestinationResolver.Resolve(
            hasCharacter: true,
            hasCompletedOnboarding: false,
            returnUrl: "/daily");

        Assert.Equal("/onboarding/tutorial", destination);
    }

    [Fact]
    public void Resolve_AfterOnboarding_UsesSafeLocalReturnUrl()
    {
        var destination = LoginDestinationResolver.Resolve(
            hasCharacter: true,
            hasCompletedOnboarding: true,
            returnUrl: "/inventory");

        Assert.Equal("/inventory", destination);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://example.com")]
    [InlineData("//example.com")]
    [InlineData("/\\example.com")]
    public void Resolve_AfterOnboarding_RejectsMissingOrUnsafeReturnUrl(string? returnUrl)
    {
        var destination = LoginDestinationResolver.Resolve(
            hasCharacter: true,
            hasCompletedOnboarding: true,
            returnUrl);

        Assert.Equal("/daily", destination);
    }
}

public sealed class LogoutDestinationResolverTests
{
    [Theory]
    [InlineData("/login", "/login")]
    [InlineData("/daily?tab=completed", "/daily?tab=completed")]
    [InlineData(null, "/login")]
    [InlineData("", "/login")]
    [InlineData("https://example.com", "/login")]
    [InlineData("//example.com", "/login")]
    [InlineData("/\\example.com", "/login")]
    public void ResolveLogout_AllowsOnlyLocalPaths(string? returnUrl, string expected)
    {
        Assert.Equal(expected, LoginDestinationResolver.ResolveLogout(returnUrl));
    }
}
