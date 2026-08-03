using LevelUp.Web.Components.DesignSystem.Layout;

namespace LevelUp.Web.Tests.Components.Layout;

public sealed class LevelUpHeaderTests
{
    [Fact]
    public void PageHeaderRendersStructuredContentAndActions()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpPageHeader>(parameters => parameters
            .Add(component => component.Eyebrow, "PLAYER")
            .Add(component => component.Title, "My Account")
            .Add(component => component.Description, "Manage your account.")
            .Add(component => component.Actions, builder => builder.AddContent(0, "Action")));

        Assert.Equal("My Account", cut.Find("h1").TextContent);
        Assert.Contains("PLAYER", cut.Find(".levelup-page-header__eyebrow").TextContent);
        Assert.Contains("Action", cut.Find(".levelup-page-header__actions").TextContent);
    }

    [Fact]
    public void SectionHeaderRendersSemanticHeading()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpSectionHeader>(parameters => parameters
            .Add(component => component.Title, "Security")
            .Add(component => component.Description, "Protect your account."));

        Assert.Equal("Security", cut.Find("h2").TextContent);
        Assert.Contains("Protect your account.", cut.Find("p").TextContent);
    }
}
