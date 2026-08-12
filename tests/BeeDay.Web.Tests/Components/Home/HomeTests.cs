using System.Text.RegularExpressions;
using BeeDay.Web.Tests.Components.Layout;
using HomePage = BeeDay.Web.Components.Features.Home.Pages.Home;

namespace BeeDay.Web.Tests.Components.Home;

public sealed class HomeTests
{
    [Fact]
    public void RendersSingleH1WithBrandMessage()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<HomePage>();

        var headings = cut.FindAll("h1");
        Assert.Single(headings);
        Assert.Equal("Be better every day", headings[0].TextContent.Trim());
    }

    [Fact]
    public void PresentsRealProductCapabilitiesOnly()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<HomePage>();

        var cardHeadings = cut.FindAll(".home-page__showcase-row h3").Select(h => h.TextContent.Trim()).ToArray();
        Assert.Equal(["Daily", "Habits", "Tasks", "Projects", "Wallet"], cardHeadings);
    }

    [Fact]
    public void DoesNotPresentFabricatedMetrics()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<HomePage>();

        Assert.False(Regex.IsMatch(cut.Markup, @"\d+\s*(%|day|days)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
        Assert.DoesNotContain("XP today", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AnonymousVisitorSeesGetStartedCta()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<HomePage>();

        Assert.Contains("Get started", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthenticatedVisitorSeesContinueCta()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetAuthorized("test-user");
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<HomePage>();

        Assert.Contains("Continue to BeeDay", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void CapabilitySectionHasStableAnchorId()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<HomePage>();

        Assert.NotNull(cut.Find("#capabilities"));
        Assert.NotNull(cut.Find("#progress"));
    }

    [Fact]
    public void HeroSecondaryActionLinksToCapabilities()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<HomePage>();

        var secondaryAction = cut.Find("a.home-hero__secondary-action");
        Assert.Equal("#capabilities", secondaryAction.GetAttribute("href"));
    }
}
