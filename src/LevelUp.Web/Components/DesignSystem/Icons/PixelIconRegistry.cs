using System.Collections.ObjectModel;

namespace LevelUp.Web.Components.DesignSystem.Icons;

public static class PixelIconRegistry
{
    public const string SpritePath = "/icons/streamline/sprite.svg";
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
                [PixelIconName.Account] = Define("user", "character/user.svg", PixelIconCategory.Navigation, "Account"),
                [PixelIconName.Language] = Define("language", "system/language.svg", PixelIconCategory.Navigation, "Language"),
                [PixelIconName.ChevronDown] = Define("chevron-down", "navigation/chevron-down.svg", PixelIconCategory.Navigation, "Expand"),
                [PixelIconName.ChevronLeft] = Define("chevron-left", "navigation/chevron-left.svg", PixelIconCategory.Navigation, "Previous"),
                [PixelIconName.ChevronRight] = Define("chevron-right", "navigation/chevron-right.svg", PixelIconCategory.Navigation, "Next"),
                [PixelIconName.Inventory] = Define("inventory", "inventory/inventory.svg", PixelIconCategory.Navigation, "Inventory"),
                [PixelIconName.Library] = Define("book", "books/book.svg", PixelIconCategory.Navigation, "Library"),
                [PixelIconName.Daily] = Define("daily", "navigation/daily.svg", PixelIconCategory.Navigation, "Daily"),
                [PixelIconName.Home] = Define("home", "navigation/home.svg", PixelIconCategory.Navigation, "Home"),
                [PixelIconName.Character] = Define("character", "character/character.svg", PixelIconCategory.Navigation, "Character"),
                [PixelIconName.Donate] = Define("donate", "navigation/donate.svg", PixelIconCategory.Navigation, "Donate"),
                [PixelIconName.Logout] = Define("logout", "navigation/logout.svg", PixelIconCategory.Navigation, "Logout"),
                [PixelIconName.Menu] = Define("menu", "navigation/menu.svg", PixelIconCategory.Navigation, "Menu"),
                [PixelIconName.Support] = Define("support", "navigation/support.svg", PixelIconCategory.Navigation, "Support"),
                [PixelIconName.Facebook] = Define("facebook", "social/facebook.svg", PixelIconCategory.Social, "Facebook"),
                [PixelIconName.Instagram] = Define("instagram", "social/instagram.svg", PixelIconCategory.Social, "Instagram"),
                [PixelIconName.YouTube] = Define("youtube", "social/youtube.svg", PixelIconCategory.Social, "YouTube"),
                [PixelIconName.X] = Define("x", "social/x.svg", PixelIconCategory.Social, "X"),
                [PixelIconName.LinkedIn] = Define("linkedin", "social/linkedin.svg", PixelIconCategory.Social, "LinkedIn"),
                [PixelIconName.GitHub] = Define("github", "social/github.svg", PixelIconCategory.Social, "GitHub"),
                [PixelIconName.Habit] = Define("habit", "habits/habit.svg", PixelIconCategory.Activities, "Habit"),
                [PixelIconName.RecurringTask] = Define("recurring-task", "tasks/recurring-task.svg", PixelIconCategory.Activities, "Recurring task"),
                [PixelIconName.Project] = Define("project", "projects/project.svg", PixelIconCategory.Activities, "Project"),
                [PixelIconName.Todo] = Define("todo", "tasks/todo.svg", PixelIconCategory.Activities, "To-Do"),
                [PixelIconName.Complete] = Define("complete", "actions/complete.svg", PixelIconCategory.Actions, "Complete"),
                [PixelIconName.Filter] = Define("filter", "actions/filter.svg", PixelIconCategory.Actions, "Filter"),
                [PixelIconName.Calendar] = Define("calendar", "actions/calendar.svg", PixelIconCategory.Actions, "Calendar"),
                [PixelIconName.Repeat] = Define("repeat", "actions/repeat.svg", PixelIconCategory.Actions, "Repeat"),
                [PixelIconName.Tag] = Define("tag", "actions/tag.svg", PixelIconCategory.Actions, "Tag"),
                [PixelIconName.Attribute] = Define("attribute", "actions/attribute.svg", PixelIconCategory.Actions, "Attribute"),
                [PixelIconName.Cancel] = Define("cancel", "actions/cancel.svg", PixelIconCategory.Actions, "Cancel"),
                [PixelIconName.Featured] = Define("featured", "feedback/featured.svg", PixelIconCategory.Feedback, "Featured"),
                [PixelIconName.Progress] = Define("progress", "statistics/progress.svg", PixelIconCategory.Statistics, "Progress"),
                [PixelIconName.Success] = Define("success", "feedback/success.svg", PixelIconCategory.Feedback, "Success"),
                [PixelIconName.ValidationError] = Define("validation-error", "feedback/validation-error.svg", PixelIconCategory.Feedback, "Validation error"),
                [PixelIconName.Loading] = Define("loading", "feedback/loading.svg", PixelIconCategory.Feedback, "Loading"),
                [PixelIconName.Select] = Define("select", "forms/select.svg", PixelIconCategory.Forms, "Select"),
                [PixelIconName.CheckboxUnchecked] = Define("checkbox-unchecked", "forms/checkbox-unchecked.svg", PixelIconCategory.Forms, "Unchecked"),
                [PixelIconName.CheckboxChecked] = Define("checkbox-checked", "forms/checkbox-checked.svg", PixelIconCategory.Forms, "Checked"),
                [PixelIconName.Experience] = Define("experience", "statistics/experience.svg", PixelIconCategory.Statistics, "Experience"),
                [PixelIconName.Level] = Define("level", "statistics/level.svg", PixelIconCategory.Statistics, "Level"),
                [PixelIconName.Wallet] = Define("wallet", "statistics/wallet.svg", PixelIconCategory.Statistics, "Wallet"),
                [PixelIconName.Income] = Define("income", "statistics/income.svg", PixelIconCategory.Statistics, "Income"),
                [PixelIconName.Expense] = Define("expense", "statistics/expense.svg", PixelIconCategory.Statistics, "Expense"),
                [PixelIconName.Statistics] = Define("statistics", "statistics/statistics.svg", PixelIconCategory.Statistics, "Statistics"),
                [PixelIconName.TrendUp] = Define("trend-up", "statistics/trend-up.svg", PixelIconCategory.Statistics, "Upward trend"),
                [PixelIconName.TrendDown] = Define("trend-down", "statistics/trend-down.svg", PixelIconCategory.Statistics, "Downward trend"),
                [PixelIconName.Streak] = Define("streak", "statistics/streak.svg", PixelIconCategory.Statistics, "Streak"),
                [PixelIconName.Completed] = Define("completed", "statistics/completed.svg", PixelIconCategory.Statistics, "Completed"),
                [PixelIconName.Pending] = Define("pending", "statistics/pending.svg", PixelIconCategory.Statistics, "Pending"),
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
        new(symbolId, $"/icons/streamline/{assetPath}", category, semanticName, semanticName, DefaultFallback);
}
