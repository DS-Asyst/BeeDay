using System.Collections.ObjectModel;

namespace LevelUp.Web.Components.DesignSystem.Icons;

public static class PixelIconRegistry
{
    public const string SpritePath = "/icons/pixel/sprite.svg";
    public const PixelIconName DefaultFallback = PixelIconName.Warning;

    private static readonly IReadOnlyDictionary<PixelIconName, PixelIconDefinition> Definitions =
        new ReadOnlyDictionary<PixelIconName, PixelIconDefinition>(
            new Dictionary<PixelIconName, PixelIconDefinition>
            {
                [PixelIconName.Add] = Define("add", "actions/add.svg", PixelIconCategory.Actions, "Add"),
                [PixelIconName.Edit] = Define("edit", "actions/edit.svg", PixelIconCategory.Actions, "Edit"),
                [PixelIconName.Delete] = Define("delete", "actions/delete.svg", PixelIconCategory.Actions, "Delete"),
                [PixelIconName.Save] = Define("save", "actions/save.svg", PixelIconCategory.Actions, "Save"),
                [PixelIconName.Close] = Define("close", "actions/close.svg", PixelIconCategory.Actions, "Close"),
                [PixelIconName.Search] = Define("search", "actions/search.svg", PixelIconCategory.Actions, "Search"),
                [PixelIconName.More] = Define("more", "actions/more.svg", PixelIconCategory.Actions, "More options"),
                [PixelIconName.Check] = Define("check", "feedback/check.svg", PixelIconCategory.Feedback, "Check"),
                [PixelIconName.Warning] = Define("warning", "feedback/warning.svg", PixelIconCategory.Feedback, "Warning"),
                [PixelIconName.Information] = Define("info", "feedback/info.svg", PixelIconCategory.Feedback, "Information"),
                [PixelIconName.Settings] = Define("settings", "system/settings.svg", PixelIconCategory.System, "Settings"),
                [PixelIconName.Lock] = Define("lock", "system/lock.svg", PixelIconCategory.System, "Lock"),
                [PixelIconName.Account] = Define("user", "navigation/user.svg", PixelIconCategory.Navigation, "Account"),
                [PixelIconName.Language] = Define("language", "navigation/language.svg", PixelIconCategory.Navigation, "Language"),
                [PixelIconName.ChevronDown] = Define("chevron-down", "navigation/chevron-down.svg", PixelIconCategory.Navigation, "Expand"),
                [PixelIconName.ChevronLeft] = Define("chevron-left", "navigation/chevron-left.svg", PixelIconCategory.Navigation, "Previous"),
                [PixelIconName.ChevronRight] = Define("chevron-right", "navigation/chevron-right.svg", PixelIconCategory.Navigation, "Next"),
                [PixelIconName.Inventory] = Define("inventory", "navigation/inventory.svg", PixelIconCategory.Navigation, "Inventory"),
                [PixelIconName.Library] = Define("book", "navigation/book.svg", PixelIconCategory.Navigation, "Library"),
                [PixelIconName.Daily] = Define("daily", "navigation/daily.svg", PixelIconCategory.Navigation, "Daily"),
                [PixelIconName.Strength] = Define("attribute-strength", "attributes/attribute-strength.svg", PixelIconCategory.Attributes, "Strength"),
                [PixelIconName.Dexterity] = Define("attribute-dexterity", "attributes/attribute-dexterity.svg", PixelIconCategory.Attributes, "Dexterity"),
                [PixelIconName.Intelligence] = Define("attribute-intelligence", "attributes/attribute-intelligence.svg", PixelIconCategory.Attributes, "Intelligence"),
                [PixelIconName.Wisdom] = Define("attribute-wisdom", "attributes/attribute-wisdom.svg", PixelIconCategory.Attributes, "Wisdom"),
                [PixelIconName.Vitality] = Define("attribute-vitality", "attributes/attribute-vitality.svg", PixelIconCategory.Attributes, "Vitality"),
                [PixelIconName.Charisma] = Define("attribute-charisma", "attributes/attribute-charisma.svg", PixelIconCategory.Attributes, "Charisma")
            });

    public static IReadOnlyDictionary<PixelIconName, PixelIconDefinition> All => Definitions;

    public static PixelIconDefinition Resolve(PixelIconName name)
    {
        if (Definitions.TryGetValue(name, out var definition))
        {
            return definition;
        }

        return Definitions[DefaultFallback];
    }

    public static bool TryGet(PixelIconName name, out PixelIconDefinition definition) =>
        Definitions.TryGetValue(name, out definition!);

    private static PixelIconDefinition Define(
        string symbolId,
        string assetPath,
        PixelIconCategory category,
        string semanticName) =>
        new(symbolId, $"/icons/pixel/{assetPath}", category, semanticName, semanticName, DefaultFallback);
}
