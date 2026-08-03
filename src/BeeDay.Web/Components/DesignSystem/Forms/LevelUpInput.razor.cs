using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Components.DesignSystem.Forms;

public partial class LevelUpInput
{
    [Parameter, EditorRequired] public string Id { get; set; } = string.Empty;
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    [Parameter] public string FieldCssClass { get; set; } = "levelup-field";
    [Parameter] public string LabelCssClass { get; set; } = "levelup-field__label";
    [Parameter] public string InputCssClass { get; set; } = "levelup-field__control";
    [Parameter] public int? MaxLength { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool ShowValidationMessage { get; set; } = true;
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter, EditorRequired] public Expression<Func<string?>> ValueExpression { get; set; } = default!;
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
}
