using System.Globalization;
using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class AppFooterTests
{
    [Fact]
    public void RendersIdentityAndOnlyRealLinks()
    {
        using var context = NewContext();
        var cut = WithEnglishUiCulture(() => context.Render<AppFooter>());

        Assert.Contains("Be Better Every Day", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("a[href='#']"));
        Assert.All(cut.FindAll("a"), link => Assert.StartsWith("https://", link.GetAttribute("href"), StringComparison.Ordinal));
    }

    [Fact]
    public void BrandReusesTheSameAssetVersionedForTheHeader()
    {
        using var context = NewContext();
        var cut = WithEnglishUiCulture(() => context.Render<AppFooter>());

        var brandMark = cut.Find("img.app-footer__brand-mark");
        Assert.Equal("/assets/brand/beeday-top-navigation.png", brandMark.GetAttribute("src"));
        Assert.Empty(cut.FindAll(".beeday-brand"));
    }

    [Fact]
    public void Tagline_UnderPortugueseUiCulture_RendersThePortugueseResource()
    {
        var restore = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
            using var context = NewContext();
            var cut = context.Render<AppFooter>();

            Assert.Contains("Seja melhor a cada dia", cut.Markup, StringComparison.Ordinal);
            Assert.DoesNotContain("Be Better Every Day", cut.Markup, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentUICulture = restore;
        }
    }

    /// <summary>AppFooter now resolves its tagline through IStringLocalizer&lt;SharedResources&gt; — every render needs AddLocalization()/AddLogging() registered, same as production's builder.Services.AddLocalization().</summary>
    private static BunitContext NewContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddLocalization();
        return context;
    }

    /// <summary>Pins the resolved culture to en-US so these tests don't depend on the running machine's default locale — see the pt-BR-specific test above for the other supported culture.</summary>
    private static T WithEnglishUiCulture<T>(Func<T> render)
    {
        var restore = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            return render();
        }
        finally
        {
            CultureInfo.CurrentUICulture = restore;
        }
    }
}
