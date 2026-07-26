using LevelUp.Web.Components.Features.Authentication.Pages;

namespace LevelUp.Web.Tests.Components.Authentication;

public sealed class LoginTests
{
    [Fact]
    public void RendersAuthenticationActionsWithSharedVisualClassAndCorrectDestinations()
    {
        using var context = new BunitContext();
        var cut = context.Render<Login>();

        var actions = cut.FindAll("a.auth-action-link");

        Assert.Equal(2, actions.Count);
        Assert.Equal("/account/forgot-password", actions[0].GetAttribute("href"));
        Assert.Contains("Forgot password?", actions[0].TextContent);
        Assert.Equal("/character/create", actions[1].GetAttribute("href"));
        Assert.Contains("Create account", actions[1].TextContent);
    }
}
