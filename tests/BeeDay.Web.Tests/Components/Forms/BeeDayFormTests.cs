using BeeDay.Web.Components.DesignSystem.Forms;
using BeeDay.Web.Tests.Localization;
using Microsoft.AspNetCore.Components.Forms;

namespace BeeDay.Web.Tests.Components.Forms;

public sealed class BeeDayFormTests
{
    [Fact]
    public void InputRendersLabelRequiredMarkerAndAttributes()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();
        var cut = RenderInsideEditContext<BeeDayInput>(context, model, parameters => parameters
            .Add(component => component.Id, "title")
            .Add(component => component.Label, "Title")
            .Add(component => component.Required, true)
            .Add(component => component.Placeholder, "Enter title")
            .Add(component => component.MaxLength, 80)
            .Add(component => component.Value, model.TextValue)
            .Add(component => component.ValueExpression, () => model.TextValue));

        var input = cut.Find("input");
        Assert.Equal("title", input.Id);
        Assert.Equal("Enter title", input.GetAttribute("placeholder"));
        Assert.Equal("80", input.GetAttribute("maxlength"));
        Assert.Contains("Title", cut.Find("label").TextContent);
        Assert.Contains("*", cut.Find("label").TextContent);
    }

    // EPIC 30 Sprint 30.20: BeeDayInput's <input> previously had no programmatic association with
    // its own validation message — a screen-reader user could not discover why a field was invalid
    // without first navigating to (and past) the separately-rendered message. aria-describedby now
    // links the two, matching the WCAG 4.1.2 "error identification" pattern already implemented
    // correctly for the message's own role="alert".
    [Fact]
    public void InputAssociatesItsValidationMessageViaAriaDescribedby()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();
        var editContext = new EditContext(model);
        var store = new ValidationMessageStore(editContext);

        var cut = context.Render<BeeDayInput>(parameters =>
        {
            parameters.AddCascadingValue(editContext);
            parameters
                .Add(component => component.Id, "title")
                .Add(component => component.Value, model.TextValue)
                .Add(component => component.ValueExpression, () => model.TextValue);
        });

        Assert.Equal("title-validation", cut.Find("input").GetAttribute("aria-describedby"));

        cut.InvokeAsync(() =>
        {
            store.Add(new FieldIdentifier(model, nameof(FormTestModel.TextValue)), "Some unmapped validation failure.");
            editContext.NotifyValidationStateChanged();
        });
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".beeday-validation-message")));

        var message = cut.Find("#title-validation");
        Assert.Contains("Some unmapped validation failure.", message.TextContent);
        Assert.Equal("title-validation", cut.Find("input").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void InputInvokesValueChanged()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();
        var cut = RenderInsideEditContext<BeeDayInput>(context, model, parameters => parameters
            .Add(component => component.Id, "title")
            .Add(component => component.Value, model.TextValue)
            .Add(component => component.ValueChanged, value => model.TextValue = value)
            .Add(component => component.ValueExpression, () => model.TextValue));

        cut.Find("input").Change("New title");

        Assert.Equal("New title", model.TextValue);
    }

    [Fact]
    public void InputCanUpdateLiveSearchWithoutAFormEditContext()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();
        var cut = context.Render<BeeDayInput>(parameters => parameters
            .Add(component => component.Id, "search")
            .Add(component => component.UpdateOnInput, true)
            .Add(component => component.ShowValidationMessage, false)
            .Add(component => component.Value, model.TextValue)
            .Add(component => component.ValueChanged, value => model.TextValue = value)
            .Add(component => component.ValueExpression, () => model.TextValue));

        cut.Find("input").Input("live value");

        Assert.Equal("live value", model.TextValue);
    }

    [Fact]
    public void InputSupportsDisabledReadonlyAndAdditionalAttributes()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();
        var cut = RenderInsideEditContext<BeeDayInput>(context, model, parameters => parameters
            .Add(component => component.Id, "title")
            .Add(component => component.Disabled, true)
            .Add(component => component.ReadOnly, true)
            .Add(component => component.Value, model.TextValue)
            .Add(component => component.ValueExpression, () => model.TextValue)
            .AddUnmatched("autocomplete", "off"));

        var input = cut.Find("input");
        Assert.True(input.HasAttribute("disabled"));
        Assert.True(input.HasAttribute("readonly"));
        Assert.Equal("off", input.GetAttribute("autocomplete"));
    }

    [Fact]
    public void TextAreaRendersCounter()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel { TextValue = "Study" };
        var cut = RenderInsideEditContext<BeeDayTextArea>(context, model, parameters => parameters
            .Add(component => component.Id, "notes")
            .Add(component => component.Label, "Notes")
            .Add(component => component.MaxLength, 100)
            .Add(component => component.ShowCounter, true)
            .Add(component => component.Value, model.TextValue)
            .Add(component => component.ValueExpression, () => model.TextValue));

        Assert.Equal("5 / 100", cut.Find(".beeday-field__counter").TextContent.Trim());
        Assert.Equal("100", cut.Find("textarea").GetAttribute("maxlength"));
    }

    [Fact]
    public void TextAreaInvokesValueChanged()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();
        var cut = RenderInsideEditContext<BeeDayTextArea>(context, model, parameters => parameters
            .Add(component => component.Id, "notes")
            .Add(component => component.Value, model.TextValue)
            .Add(component => component.ValueChanged, value => model.TextValue = value)
            .Add(component => component.ValueExpression, () => model.TextValue));

        cut.Find("textarea").Change("Updated notes");

        Assert.Equal("Updated notes", model.TextValue);
    }

    [Fact]
    public void CheckboxRendersLabelAndInvokesValueChanged()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();
        var cut = RenderInsideEditContext<BeeDayCheckbox>(context, model, parameters => parameters
            .Add(component => component.Id, "favorite")
            .Add(component => component.Label, "Favorite")
            .Add(component => component.Value, model.BoolValue)
            .Add(component => component.ValueChanged, value => model.BoolValue = value)
            .Add(component => component.ValueExpression, () => model.BoolValue));

        Assert.Contains("Favorite", cut.Find("label").TextContent);
        cut.Find("input").Change(true);
        Assert.True(model.BoolValue);
    }

    [Fact]
    public void CheckboxSupportsDisabledState()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();
        var cut = RenderInsideEditContext<BeeDayCheckbox>(context, model, parameters => parameters
            .Add(component => component.Id, "favorite")
            .Add(component => component.Label, "Favorite")
            .Add(component => component.Disabled, true)
            .Add(component => component.Value, model.BoolValue)
            .Add(component => component.ValueExpression, () => model.BoolValue));

        Assert.True(cut.Find("input").HasAttribute("disabled"));
    }

    [Fact]
    public void DateInputRendersAndInvokesValueChanged()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();
        var cut = RenderInsideEditContext<BeeDayDateInput<DateTime?>>(context, model, parameters => parameters
            .Add(component => component.Id, "due-date")
            .Add(component => component.Label, "Due date")
            .Add(component => component.Value, model.DateValue)
            .Add(component => component.ValueChanged, value => model.DateValue = value)
            .Add(component => component.ValueExpression, () => model.DateValue));

        cut.Find("input").Change("2026-08-10");

        Assert.Equal(new DateTime(2026, 8, 10), model.DateValue);
    }

    // EPIC 32 Sprint 32.5 (EXP32-F005): the native <input type="date"> picker/placeholder follows
    // the browser/OS locale by default, not beeday's selected culture — a pt-BR OS showed
    // "dd/mm/aaaa" even with the rest of the UI in English. Chromium honors an element's own `lang`
    // attribute for this formatting, so pinning it to the app's current culture keeps the date
    // control consistent with the rest of the localized page.
    [Theory]
    [InlineData("en-US")]
    [InlineData("pt-BR")]
    public void DateInput_LangAttributeFollowsCurrentCulture_NotTheMachineDefault(string culture)
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel();

        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => RenderInsideEditContext<BeeDayDateInput<DateTime?>>(context, model, parameters => parameters
            .Add(component => component.Id, "due-date")
            .Add(component => component.Label, "Due date")
            .Add(component => component.Value, model.DateValue)
            .Add(component => component.ValueChanged, value => model.DateValue = value)
            .Add(component => component.ValueExpression, () => model.DateValue)));

        Assert.Equal(culture, cut.Find("input").GetAttribute("lang"));
    }

    [Fact]
    public void SelectRendersOptionsAndInvokesValueChanged()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var model = new FormTestModel { SelectValue = "medium" };
        var cut = RenderInsideEditContext<BeeDaySelect<string>>(context, model, parameters => parameters
            .Add(component => component.Id, "difficulty")
            .Add(component => component.Label, "Difficulty")
            .Add(component => component.Value, model.SelectValue)
            .Add(component => component.ValueChanged, value => model.SelectValue = value)
            .Add(component => component.ValueExpression, () => model.SelectValue)
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "option");
                builder.AddAttribute(1, "value", "easy");
                builder.AddContent(2, "Easy");
                builder.CloseElement();
                builder.OpenElement(3, "option");
                builder.AddAttribute(4, "value", "hard");
                builder.AddContent(5, "Hard");
                builder.CloseElement();
            }));

        cut.Find("select").Change("hard");

        Assert.Equal("hard", model.SelectValue);
        Assert.Equal(2, cut.FindAll("option").Count);
    }

    private static IRenderedComponent<TComponent> RenderInsideEditContext<TComponent>(
        BunitContext context,
        FormTestModel model,
        Action<ComponentParameterCollectionBuilder<TComponent>> configure)
        where TComponent : Microsoft.AspNetCore.Components.IComponent
    {
        var editContext = new EditContext(model);

        return context.Render<TComponent>(parameters =>
        {
            parameters.AddCascadingValue(editContext);
            configure(parameters);
        });
    }

    private sealed class FormTestModel
    {
        public string? TextValue { get; set; }

        public string SelectValue { get; set; } = string.Empty;

        public bool BoolValue { get; set; }

        public DateTime? DateValue { get; set; }
    }
}
