using BeeDay.Web.Services.Authentication;

namespace BeeDay.Web.Tests.Services;

public sealed class LoginDestinationResolverTests
{
    [Fact]
    public void Resolve_WithoutProfile_RequiresProfileCreation()
    {
        var destination = LoginDestinationResolver.Resolve(
            hasProfile: false,
            hasCompletedOnboarding: false,
            returnUrl: "/daily");

        Assert.Equal("/profile/create", destination);
    }

    [Fact]
    public void Resolve_FirstLoginWithProfile_RequiresTutorialEvenWhenReturnUrlTargetsDaily()
    {
        var destination = LoginDestinationResolver.Resolve(
            hasProfile: true,
            hasCompletedOnboarding: false,
            returnUrl: "/daily");

        Assert.Equal("/onboarding/tutorial", destination);
    }

    [Fact]
    public void Resolve_AfterOnboarding_UsesSafeLocalReturnUrl()
    {
        var destination = LoginDestinationResolver.Resolve(
            hasProfile: true,
            hasCompletedOnboarding: true,
            returnUrl: "/wallet");

        Assert.Equal("/wallet", destination);
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
            hasProfile: true,
            hasCompletedOnboarding: true,
            returnUrl);

        Assert.Equal("/home", destination);
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
