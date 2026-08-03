using BeeDay.Web.Components.DesignSystem.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class BeeDaySettingsTests
{
    private sealed class DummyModel
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void SettingsSectionRendersHeaderAndChildContent()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDaySettingsSection>(parameters => parameters
            .Add(component => component.Eyebrow, "IDENTITY")
            .Add(component => component.Title, "Profile")
            .Add(component => component.Description, "Manage your profile.")
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Form goes here")));

        Assert.Contains("beeday-card", cut.Find("article").ClassList);
        Assert.Equal("Profile", cut.Find("h2").TextContent);
        Assert.Contains("IDENTITY", cut.Find(".beeday-settings-section__header").TextContent, StringComparison.Ordinal);
        Assert.Contains("Form goes here", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SettingsFormInvokesOnValidSubmitAndShowsSubmitLabel()
    {
        using var context = new BunitContext();
        var submitted = false;

        var cut = context.Render<BeeDaySettingsForm<DummyModel>>(parameters => parameters
            .Add(component => component.Model, new DummyModel())
            .Add(component => component.FormName, "test-form")
            .Add(component => component.SubmitLabel, "SAVE THING")
            .Add(component => component.OnValidSubmit, () => submitted = true));

        Assert.Contains("SAVE THING", cut.Find("button[type='submit']").TextContent, StringComparison.Ordinal);

        await cut.Find("form").SubmitAsync();

        Assert.True(submitted);
    }

    [Fact]
    public void SettingsFormDisablesFieldsetWhileBusy()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDaySettingsForm<DummyModel>>(parameters => parameters
            .Add(component => component.Model, new DummyModel())
            .Add(component => component.FormName, "test-form")
            .Add(component => component.SubmitLabel, "SAVE THING")
            .Add(component => component.IsBusy, true));

        Assert.True(cut.Find("fieldset").HasAttribute("disabled"));
    }
}
