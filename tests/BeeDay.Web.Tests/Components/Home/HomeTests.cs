using System.Text.RegularExpressions;
using BeeDay.Web.Tests.Components.Layout;
using HomePage = BeeDay.Web.Components.Features.Home.Pages.Home;

namespace BeeDay.Web.Tests.Components.Home;

public sealed class HomeTests
{
    [Fact]
    public void RendersSingleH1WithProductPromise()
    {
        using var context = CreateContext();
        var cut = context.Render<HomePage>();

        var headings = cut.FindAll("h1");
        Assert.Single(headings);
        Assert.Contains("one step at a time", headings[0].TextContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AnonymousCallsToActionTargetRegistrationAndLogin()
    {
        using var context = CreateContext();
        var cut = context.Render<HomePage>();

        Assert.Single(cut.FindAll("a[href='/profile/create']"));
        Assert.NotNull(cut.Find(".home-hero__login[href='/login']"));
        Assert.Contains("beeday-button--secondary", cut.Find(".home-hero__login").ClassList);
    }

    [Fact]
    public void PresentsRealProductConceptsAndSimpleProcess()
    {
        using var context = CreateContext();
        var cut = context.Render<HomePage>();

        Assert.Equal(["Define", "Practice", "Evolve"], cut.FindAll(".home-steps h3").Select(element => element.TextContent.Trim()));
        Assert.NotNull(cut.Find(".home-hero__visual canvas"));
        Assert.Empty(cut.FindAll(".home-hero .beeday-brand"));
        Assert.Empty(cut.FindAll(".home-hero__symbol"));
        Assert.Empty(cut.FindAll(".home-preview, .home-values, .home-growth, .home-cta"));
    }

    [Fact]
    public void DoesNotPresentUnsupportedGamificationOrFabricatedMetrics()
    {
        using var context = CreateContext();
        var cut = context.Render<HomePage>();

        Assert.False(Regex.IsMatch(cut.Markup, @"\d+\s*(%|day|days)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
        Assert.DoesNotContain("streak", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("achievement", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("XP today", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AuthenticatedVisitorSeesContinueCta()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.AddAuthorization().SetAuthorized("test-user");
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<HomePage>();

        Assert.Contains("Continue to BeeDay", cut.Markup, StringComparison.Ordinal);
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.AddAuthorization().SetNotAuthorized();
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);
        return context;
    }
}
