using BeeDay.Web.Components.Layout;
using BeeDay.Web.Resources;
using BeeDay.Web.Services;
using BeeDay.Web.Tests.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class PublicLayoutTests
{
    [Fact]
    public void RendersHeaderMainBodyAndFooter()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetNotAuthorized();
        context.Services.AddSingleton(sp => new ToastService(sp.GetRequiredService<IStringLocalizer<SharedResources>>()));
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        RenderFragment body = builder =>
        {
            builder.OpenElement(0, "p");
            builder.AddContent(1, "page content");
            builder.CloseElement();
        };

        var cut = context.Render<PublicLayout>(parameters => parameters
            .Add(component => component.Body, body));

        Assert.NotNull(cut.Find("header.public-header"));
        Assert.NotNull(cut.Find("main.beeday-main"));
        Assert.NotNull(cut.Find("footer.app-footer"));
        Assert.Contains("page content", cut.Find("main").TextContent);
    }

    // EXP32-F008 (Sprint 32.13): the skip link's own href must resolve to the actual <main> the
    // layout renders, not just exist as an unconnected link pointing nowhere.
    [Fact]
    public void SkipLinkTargetsTheRenderedMainContentRegion()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetNotAuthorized();
        context.Services.AddSingleton(sp => new ToastService(sp.GetRequiredService<IStringLocalizer<SharedResources>>()));
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = context.Render<PublicLayout>(parameters => parameters
            .Add(component => component.Body, (RenderFragment)(builder => builder.AddContent(0, "page content"))));

        var skipLink = cut.Find(".skip-to-content-link");
        Assert.Equal("#main-content", skipLink.GetAttribute("href"));
        Assert.Equal("main-content", cut.Find("main").Id);
    }
}
