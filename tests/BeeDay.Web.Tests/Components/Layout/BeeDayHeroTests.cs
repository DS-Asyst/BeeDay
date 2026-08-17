using BeeDay.Web.Components.DesignSystem;
using BeeDay.Web.Components.DesignSystem.Icons;
using BeeDay.Web.Components.DesignSystem.Layout;
using Microsoft.AspNetCore.Components.Rendering;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class BeeDayHeroTests
{
    [Fact]
    public void RendersRequiredTitleAsHeading1()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Inventory"));

        Assert.Equal("Inventory", cut.Find("h1").TextContent);
    }

    [Fact]
    public void RendersEyebrowAndSubtitleWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Inventory")
            .Add(component => component.Eyebrow, "Personal finance")
            .Add(component => component.Subtitle, "Track your wallet."));

        Assert.Contains("Personal finance", cut.Find(".beeday-hero__eyebrow").TextContent);
        Assert.Contains("Track your wallet.", cut.Find(".beeday-hero__subtitle").TextContent);
    }

    [Fact]
    public void OmitsEyebrowAndSubtitleWrappersWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Inventory"));

        Assert.Empty(cut.FindAll(".beeday-hero__eyebrow"));
        Assert.Empty(cut.FindAll(".beeday-hero__subtitle"));
    }

    [Fact]
    public void RendersIllustrationWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Illustration, (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent<BeeDayIcon>(0);
                builder.AddAttribute(1, "Name", BeeDayIconName.Daily);
                builder.CloseComponent();
            }));

        var illustration = cut.Find(".beeday-hero__illustration");
        Assert.NotNull(illustration.QuerySelector("svg"));
    }

    [Fact]
    public void OmitsIllustrationWrapperWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Daily"));

        Assert.Empty(cut.FindAll(".beeday-hero__illustration"));
    }

    [Fact]
    public void SupportsADecorativeIllustration()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Illustration, (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent<BeeDayIcon>(0);
                builder.AddAttribute(1, "Name", BeeDayIconName.Profile);
                builder.AddAttribute(2, "Decorative", true);
                builder.CloseComponent();
            }));

        var svg = cut.Find(".beeday-hero__illustration svg");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void SupportsAnInformativeIllustrationWithAlternativeText()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Illustration, (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent<BeeDayIcon>(0);
                builder.AddAttribute(1, "Name", BeeDayIconName.Warning);
                builder.AddAttribute(2, "Decorative", false);
                builder.AddAttribute(3, "Label", "Action required");
                builder.CloseComponent();
            }));

        var svg = cut.Find(".beeday-hero__illustration svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Action required", svg.GetAttribute("aria-label"));
    }

    [Fact]
    public void RendersPrimaryActionWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Inventory")
            .Add(component => component.PrimaryAction, builder => builder.AddContent(0, "New transaction")));

        Assert.Contains("New transaction", cut.Find(".beeday-hero__primary-action").TextContent);
    }

    [Fact]
    public void OmitsPrimaryActionWrapperWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Inventory"));

        Assert.Empty(cut.FindAll(".beeday-hero__primary-action"));
    }

    [Fact]
    public void RendersSupportingContentWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.SupportingContent, builder => builder.AddContent(0, "Return throughout the day.")));

        Assert.Contains("Return throughout the day.", cut.Find(".beeday-hero__supporting").TextContent);
    }

    [Fact]
    public void OmitsSupportingContentWrapperWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Daily"));

        Assert.Empty(cut.FindAll(".beeday-hero__supporting"));
    }

    [Theory]
    [InlineData(BeeDayHeroVariant.Default, false)]
    [InlineData(BeeDayHeroVariant.Onboarding, true)]
    public void AppliesVariantClass(BeeDayHeroVariant variant, bool expectsOnboardingClass)
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Variant, variant));

        var header = cut.Find("header");
        Assert.Contains("beeday-hero", header.ClassList);
        Assert.Equal(expectsOnboardingClass, header.ClassList.Contains("beeday-hero--onboarding"));
    }

    [Fact]
    public void MergesCustomClassAndAdditionalAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Class, "custom-hero")
            .AddUnmatched("data-testid", "daily-hero"));

        var header = cut.Find("header");
        Assert.Contains("custom-hero", header.ClassList);
        Assert.Equal("daily-hero", header.GetAttribute("data-testid"));
    }

    [Theory]
    [InlineData(BeeDayPaletteToken.Cor0, "beeday-surface-cor0")]
    [InlineData(BeeDayPaletteToken.Cor3, "beeday-surface-cor3")]
    [InlineData(BeeDayPaletteToken.Cor8, "beeday-surface-cor8")]
    public void AppliesTheSurfacePairingClassWhenSet(BeeDayPaletteToken surface, string expectedClass)
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Mission")
            .Add(component => component.Surface, surface));

        Assert.Contains(expectedClass, cut.Find("header").ClassList);
    }

    [Fact]
    public void OmitsSurfaceClassWhenNotSet()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Inventory"));

        Assert.DoesNotContain(cut.Find("header").ClassList, cssClass => cssClass.StartsWith("beeday-surface-", StringComparison.Ordinal));
    }

    [Fact]
    public void AppliesCompactModifierClassWhenRequested()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Wallet")
            .Add(component => component.Compact, true));

        Assert.Contains("beeday-hero--compact", cut.Find("header").ClassList);
    }

    [Fact]
    public void RendersBrandContextWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Mission")
            .Add(component => component.BrandContext, builder => builder.AddContent(0, "beeday Mission")));

        Assert.Contains("beeday Mission", cut.Find(".beeday-hero__brand-context").TextContent);
    }

    [Fact]
    public void OmitsBrandContextWrapperWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Mission"));

        Assert.Empty(cut.FindAll(".beeday-hero__brand-context"));
    }

    [Fact]
    public void RendersOnlyTheHeadingWhenAllOptionalParametersAreOmitted()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayHero>(parameters => parameters
            .Add(component => component.Title, "Title-only Hero"));

        Assert.Empty(cut.FindAll(".beeday-hero__brand-context"));
        Assert.Empty(cut.FindAll(".beeday-hero__eyebrow"));
        Assert.Empty(cut.FindAll(".beeday-hero__subtitle"));
        Assert.Empty(cut.FindAll(".beeday-hero__illustration"));
        Assert.Empty(cut.FindAll(".beeday-hero__primary-action"));
        Assert.Empty(cut.FindAll(".beeday-hero__supporting"));
        Assert.Single(cut.FindAll("h1"));
    }
}
