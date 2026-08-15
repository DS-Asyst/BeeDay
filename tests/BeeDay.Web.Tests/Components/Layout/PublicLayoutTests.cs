using BeeDay.Web.Components.Layout;
using BeeDay.Web.Services;
using BeeDay.Web.Tests.Localization;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class PublicLayoutTests
{
    [Fact]
    public void RendersHeaderMainBodyAndFooter()
    {
        using var context = new BunitContext().WithLocalization();
        context.AddAuthorization().SetNotAuthorized();
        context.Services.AddSingleton(new ToastService());
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
}
