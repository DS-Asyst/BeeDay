using BeeDay.Web.Components.Features.Authentication.Components;

namespace BeeDay.Web.Tests.Components.Authentication;

public sealed class PublicAuthActionsTests
{
    [Fact]
    public void RendersExplicitHomeAndDestinationRoutes()
    {
        using var context = new BunitContext();
        var cut = context.Render<PublicAuthActions>(parameters => parameters
            .Add(component => component.DestinationHref, "/login")
            .Add(component => component.DestinationLabel, "Log in")
            .Add(component => component.CloseLabel, "Close create account and return to Home"));

        Assert.Equal("/", cut.Find("a.public-auth-actions__close").GetAttribute("href"));
        Assert.Equal("Close create account and return to Home", cut.Find("a.public-auth-actions__close").GetAttribute("aria-label"));
        Assert.Equal("/login", cut.Find("a.public-auth-actions__destination").GetAttribute("href"));
    }
}
