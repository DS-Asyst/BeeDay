using LevelUp.Web.Components.DesignSystem.Cards;

namespace LevelUp.Web.Tests.Components.Cards;

public sealed class LevelUpCardMenuTests
{
    [Fact]
    public void StartsClosedWithAccessibleLabel()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpCardMenu>(parameters => parameters
            .Add(component => component.Title, "Read a book"));

        var trigger = cut.Find("button");
        Assert.Equal("Options for Read a book", trigger.GetAttribute("aria-label"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("[role='menu']"));
    }

    [Fact]
    public void OpensAndClosesMenu()
    {
        using var context = new BunitContext();
        var states = new List<bool>();
        var cut = context.Render<LevelUpCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task")
            .Add(component => component.OpenChanged, value => states.Add(value)));

        cut.Find("button").Click();
        Assert.Single(cut.FindAll("[role='menu']"));
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));

        cut.Find(".card-action-menu__dismiss").Click();
        Assert.Empty(cut.FindAll("[role='menu']"));
        Assert.Equal([true, false], states);
    }

    [Fact]
    public void EditClosesMenuAndInvokesCallback()
    {
        using var context = new BunitContext();
        var edited = false;
        var cut = context.Render<LevelUpCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task")
            .Add(component => component.OnEdit, () => edited = true));

        cut.Find("button").Click();
        cut.FindAll("[role='menuitem']")[0].Click();

        Assert.True(edited);
        Assert.Empty(cut.FindAll("[role='menu']"));
    }

    [Fact]
    public void DeleteClosesMenuAndInvokesCallback()
    {
        using var context = new BunitContext();
        var deleted = false;
        var cut = context.Render<LevelUpCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task")
            .Add(component => component.OnDelete, () => deleted = true));

        cut.Find("button").Click();
        cut.FindAll("[role='menuitem']")[1].Click();

        Assert.True(deleted);
        Assert.Empty(cut.FindAll("[role='menu']"));
    }
}
