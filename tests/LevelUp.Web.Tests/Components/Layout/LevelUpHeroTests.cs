using LevelUp.Web.Components.DesignSystem.Icons;
using LevelUp.Web.Components.DesignSystem.Layout;
using Microsoft.AspNetCore.Components.Rendering;

namespace LevelUp.Web.Tests.Components.Layout;

public sealed class LevelUpHeroTests
{
    [Fact]
    public void RendersRequiredTitleAsHeading1()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Inventory"));

        Assert.Equal("Inventory", cut.Find("h1").TextContent);
    }

    [Fact]
    public void RendersEyebrowAndSubtitleWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Inventory")
            .Add(component => component.Eyebrow, "Personal finance")
            .Add(component => component.Subtitle, "Track your wallet."));

        Assert.Contains("Personal finance", cut.Find(".levelup-hero__eyebrow").TextContent);
        Assert.Contains("Track your wallet.", cut.Find(".levelup-hero__subtitle").TextContent);
    }

    [Fact]
    public void OmitsEyebrowAndSubtitleWrappersWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Inventory"));

        Assert.Empty(cut.FindAll(".levelup-hero__eyebrow"));
        Assert.Empty(cut.FindAll(".levelup-hero__subtitle"));
    }

    [Fact]
    public void RendersIllustrationWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Illustration, (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent<PixelIcon>(0);
                builder.AddAttribute(1, "Name", PixelIconName.Daily);
                builder.CloseComponent();
            }));

        var illustration = cut.Find(".levelup-hero__illustration");
        Assert.NotNull(illustration.QuerySelector("svg"));
    }

    [Fact]
    public void OmitsIllustrationWrapperWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Daily"));

        Assert.Empty(cut.FindAll(".levelup-hero__illustration"));
    }

    [Fact]
    public void SupportsADecorativeIllustration()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Illustration, (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent<PixelIcon>(0);
                builder.AddAttribute(1, "Name", PixelIconName.Character);
                builder.AddAttribute(2, "Decorative", true);
                builder.CloseComponent();
            }));

        var svg = cut.Find(".levelup-hero__illustration svg");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void SupportsAnInformativeIllustrationWithAlternativeText()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Illustration, (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent<PixelIcon>(0);
                builder.AddAttribute(1, "Name", PixelIconName.Warning);
                builder.AddAttribute(2, "Decorative", false);
                builder.AddAttribute(3, "Label", "Action required");
                builder.CloseComponent();
            }));

        var svg = cut.Find(".levelup-hero__illustration svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Action required", svg.GetAttribute("aria-label"));
    }

    [Fact]
    public void RendersPrimaryActionWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Inventory")
            .Add(component => component.PrimaryAction, builder => builder.AddContent(0, "New transaction")));

        Assert.Contains("New transaction", cut.Find(".levelup-hero__primary-action").TextContent);
    }

    [Fact]
    public void OmitsPrimaryActionWrapperWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Inventory"));

        Assert.Empty(cut.FindAll(".levelup-hero__primary-action"));
    }

    [Fact]
    public void RendersSupportingContentWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.SupportingContent, builder => builder.AddContent(0, "Return throughout the day.")));

        Assert.Contains("Return throughout the day.", cut.Find(".levelup-hero__supporting").TextContent);
    }

    [Fact]
    public void OmitsSupportingContentWrapperWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Daily"));

        Assert.Empty(cut.FindAll(".levelup-hero__supporting"));
    }

    [Theory]
    [InlineData(LevelUpHeroVariant.Default, false)]
    [InlineData(LevelUpHeroVariant.Onboarding, true)]
    public void AppliesVariantClass(LevelUpHeroVariant variant, bool expectsOnboardingClass)
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Variant, variant));

        var header = cut.Find("header");
        Assert.Contains("levelup-hero", header.ClassList);
        Assert.Equal(expectsOnboardingClass, header.ClassList.Contains("levelup-hero--onboarding"));
    }

    [Fact]
    public void MergesCustomClassAndAdditionalAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Daily")
            .Add(component => component.Class, "custom-hero")
            .AddUnmatched("data-testid", "daily-hero"));

        var header = cut.Find("header");
        Assert.Contains("custom-hero", header.ClassList);
        Assert.Equal("daily-hero", header.GetAttribute("data-testid"));
    }

    [Fact]
    public void RendersOnlyTheHeadingWhenAllOptionalParametersAreOmitted()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpHero>(parameters => parameters
            .Add(component => component.Title, "Title-only Hero"));

        Assert.Empty(cut.FindAll(".levelup-hero__eyebrow"));
        Assert.Empty(cut.FindAll(".levelup-hero__subtitle"));
        Assert.Empty(cut.FindAll(".levelup-hero__illustration"));
        Assert.Empty(cut.FindAll(".levelup-hero__primary-action"));
        Assert.Empty(cut.FindAll(".levelup-hero__supporting"));
        Assert.Single(cut.FindAll("h1"));
    }
}
