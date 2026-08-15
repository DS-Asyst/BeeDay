using BeeDay.Web.Components.DesignSystem.Cards;
using BeeDay.Web.Services;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Cards;

public sealed class BeeDayCardMenuTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext().WithLocalization();
        context.Services.AddScoped<CardActionMenuCoordinator>();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void StartsClosedWithAccessibleLabel()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Read a book")));

        var trigger = cut.Find("button");
        Assert.Equal("Options for Read a book", trigger.GetAttribute("aria-label"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("[role='menu']"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseLabelsAndFormattedAriaLabel()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Ler um livro")));

        Assert.Equal("Opções para Ler um livro", cut.Find("button").GetAttribute("aria-label"));

        cut.Find("button").Click();
        var items = cut.FindAll("[role='menuitem'] span");
        Assert.Equal(["EDITAR", "EXCLUIR"], items.Select(item => item.TextContent));
    }

    [Fact]
    public void OpensMenuOnTriggerClick()
    {
        using var context = CreateContext();
        var states = new List<bool>();
        var cut = context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task")
            .Add(component => component.OpenChanged, value => states.Add(value)));

        cut.Find("button").Click();
        Assert.Single(cut.FindAll("[role='menu']"));
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
        Assert.Equal([true], states);
    }

    [Fact]
    public async Task OutsideClickNotificationFromJsClosesMenu()
    {
        using var context = CreateContext();
        var states = new List<bool>();
        var cut = context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task")
            .Add(component => component.OpenChanged, value => states.Add(value)));

        cut.Find("button").Click();
        Assert.Single(cut.FindAll("[role='menu']"));

        // Simulates the real document-level pointerdown listener (registered
        // via beeday-card-menu.js) invoking back into .NET when a click
        // lands outside the menu's root element.
        await cut.InvokeAsync(() => cut.Instance.NotifyOutsideClickAsync());
        cut.Render();

        Assert.Empty(cut.FindAll("[role='menu']"));
        Assert.Equal([true, false], states);
    }

    [Fact]
    public void ClickingTriggerAgainClosesMenu()
    {
        using var context = CreateContext();
        var cut = context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task"));

        cut.Find("button").Click();
        Assert.Single(cut.FindAll("[role='menu']"));

        cut.Find("button").Click();
        Assert.Empty(cut.FindAll("[role='menu']"));
    }

    [Fact]
    public void EscapeClosesMenu()
    {
        using var context = CreateContext();
        var cut = context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task"));

        cut.Find("button").Click();
        Assert.Single(cut.FindAll("[role='menu']"));

        cut.Find(".card-action-menu").KeyDown("Escape");
        Assert.Empty(cut.FindAll("[role='menu']"));
    }

    [Fact]
    public void EditClosesMenuAndInvokesCallback()
    {
        using var context = CreateContext();
        var edited = false;
        var cut = context.Render<BeeDayCardMenu>(parameters => parameters
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
        using var context = CreateContext();
        var deleted = false;
        var cut = context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task")
            .Add(component => component.OnDelete, () => deleted = true));

        cut.Find("button").Click();
        cut.FindAll("[role='menuitem']")[1].Click();

        Assert.True(deleted);
        Assert.Empty(cut.FindAll("[role='menu']"));
    }

    [Fact]
    public void OnlyOneMenuIsOpenAtATime()
    {
        using var context = CreateContext();

        var cutA = context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Card A"));
        var cutB = context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Card B"));

        cutA.Find("button").Click();
        Assert.Single(cutA.FindAll("[role='menu']"));

        cutB.Find("button").Click();
        Assert.Single(cutB.FindAll("[role='menu']"));
        Assert.Empty(cutA.FindAll("[role='menu']"));
    }

    [Fact]
    public void PlacesPanelBelowByDefaultWhenViewportSpaceIsSufficient()
    {
        using var context = CreateContext();
        context.JSInterop.SetupModule("./js/beeday-card-menu.js?v=20260729-1")
            .Setup<CardMenuGeometry>("measureGeometry", _ => true)
            .SetResult(new CardMenuGeometry(
                TriggerTop: 100, TriggerBottom: 120, TriggerLeft: 50, TriggerRight: 100,
                MenuWidth: 140, MenuHeight: 80,
                ViewportWidth: 1000, ViewportHeight: 800));

        var cut = context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task"));

        cut.Find("button").Click();

        var panel = cut.Find(".card-action-menu__panel");
        Assert.DoesNotContain("card-action-menu__panel--flip-up", panel.ClassList);
        Assert.DoesNotContain("card-action-menu__panel--measuring", panel.ClassList);
    }

    [Fact]
    public void FlipsPanelAboveWhenViewportSpaceBelowIsInsufficient()
    {
        using var context = CreateContext();
        context.JSInterop.SetupModule("./js/beeday-card-menu.js?v=20260729-1")
            .Setup<CardMenuGeometry>("measureGeometry", _ => true)
            .SetResult(new CardMenuGeometry(
                TriggerTop: 750, TriggerBottom: 770, TriggerLeft: 50, TriggerRight: 100,
                MenuWidth: 140, MenuHeight: 80,
                ViewportWidth: 1000, ViewportHeight: 800));

        var cut = context.Render<BeeDayCardMenu>(parameters => parameters
            .Add(component => component.Title, "Task"));

        cut.Find("button").Click();

        var panel = cut.Find(".card-action-menu__panel");
        Assert.Contains("card-action-menu__panel--flip-up", panel.ClassList);
        Assert.DoesNotContain("card-action-menu__panel--measuring", panel.ClassList);
    }
}
