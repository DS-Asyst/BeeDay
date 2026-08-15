using BeeDay.Web.Components.Features.Authentication.Pages;

namespace BeeDay.Web.Tests.Components.Authentication;

public sealed class LoginTests
{
    [Fact]
    public void RendersFullscreenNavigationActionsWithCorrectDestinations()
    {
        using var context = new BunitContext();
        var cut = context.Render<Login>();

        var close = cut.Find("a.public-auth-actions__close");
        Assert.Equal("/", close.GetAttribute("href"));
        Assert.Equal("Close login and return to Home", close.GetAttribute("aria-label"));

        var create = cut.Find("a.public-auth-actions__destination");
        Assert.Equal("/profile/create", create.GetAttribute("href"));
        Assert.Contains("Create account", create.TextContent, StringComparison.Ordinal);
        Assert.Contains("beeday-button--secondary", create.ClassList);

        var forgotPassword = cut.Find("a.auth-action-link");
        Assert.Equal("/account/forgot-password", forgotPassword.GetAttribute("href"));
        Assert.Contains("Forgot password?", forgotPassword.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void PreservesAuthenticationContractAndRemovesCardContainer()
    {
        using var context = new BunitContext();
        var cut = context.Render<Login>();

        Assert.Empty(cut.FindAll(".auth-card"));
        Assert.Equal("/auth/login", cut.Find("form[method='post']").GetAttribute("action"));
        Assert.NotNull(cut.Find("input[name='rememberMe']"));
        Assert.NotNull(cut.Find("input[name='returnUrl']"));
    }
}
