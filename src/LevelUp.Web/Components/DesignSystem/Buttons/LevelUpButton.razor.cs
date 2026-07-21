using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Buttons;

public partial class LevelUpButton
{
    [Parameter] public LevelUpButtonVariant Variant { get; set; } = LevelUpButtonVariant.Primary;
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClasses
    {
        get
        {
            var variantClass = Variant switch
            {
                LevelUpButtonVariant.Primary => "editor-modal__save",
                LevelUpButtonVariant.Secondary => "editor-modal__cancel",
                LevelUpButtonVariant.Danger => "editor-modal__delete",
                LevelUpButtonVariant.ConfirmationDanger => "delete-confirmation__delete-button",
                LevelUpButtonVariant.ConfirmationCancel => "delete-confirmation__cancel-button",
                _ => string.Empty
            };

            return string.Join(' ', new[] { variantClass, Class }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }
}
