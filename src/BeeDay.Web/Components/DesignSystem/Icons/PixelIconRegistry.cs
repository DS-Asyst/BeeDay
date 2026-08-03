using System.Collections.ObjectModel;

namespace LevelUp.Web.Components.DesignSystem.Icons;

public static class PixelIconRegistry
{
    public const string SpritePath = "/icons/sprite.svg";
    public const PixelIconName DefaultFallback = PixelIconName.Warning;

    private static readonly IReadOnlyDictionary<PixelIconName, PixelIconDefinition> Definitions =
        new ReadOnlyDictionary<PixelIconName, PixelIconDefinition>(
            new Dictionary<PixelIconName, PixelIconDefinition>
            {
                [PixelIconName.Add] = Define("add", "material-symbols/actions/add.svg", PixelIconCategory.Actions, "Add"),
                [PixelIconName.Edit] = Define("edit", "material-symbols/actions/edit.svg", PixelIconCategory.Actions, "Edit"),
                [PixelIconName.Delete] = Define("delete", "material-symbols/actions/delete.svg", PixelIconCategory.Actions, "Delete"),
                [PixelIconName.Save] = Define("save", "material-symbols/actions/save.svg", PixelIconCategory.Actions, "Save"),
                [PixelIconName.Close] = Define("close", "material-symbols/actions/close.svg", PixelIconCategory.Actions, "Close"),
                [PixelIconName.Search] = Define("search", "material-symbols/actions/search.svg", PixelIconCategory.Actions, "Search"),
                [PixelIconName.More] = Define("more", "material-symbols/actions/more.svg", PixelIconCategory.Actions, "More options"),
                [PixelIconName.Check] = Define("check", "material-symbols/feedback/check.svg", PixelIconCategory.Feedback, "Check"),
                [PixelIconName.Warning] = Define("warning", "material-symbols/feedback/warning.svg", PixelIconCategory.Feedback, "Warning"),
                [PixelIconName.Information] = Define("info", "material-symbols/feedback/info.svg", PixelIconCategory.Feedback, "Information"),
                [PixelIconName.Settings] = Define("settings", "material-symbols/system/settings.svg", PixelIconCategory.System, "Settings"),
                [PixelIconName.Lock] = Define("lock", "material-symbols/system/lock.svg", PixelIconCategory.System, "Lock"),
                [PixelIconName.Account] = Define("user", "material-symbols/profile/user.svg", PixelIconCategory.Navigation, "Account"),
                [PixelIconName.Language] = Define("language", "material-symbols/system/language.svg", PixelIconCategory.Navigation, "Language"),
                [PixelIconName.ChevronDown] = Define("chevron-down", "material-symbols/navigation/chevron-down.svg", PixelIconCategory.Navigation, "Expand"),
                [PixelIconName.ChevronLeft] = Define("chevron-left", "material-symbols/navigation/chevron-left.svg", PixelIconCategory.Navigation, "Previous"),
                [PixelIconName.ChevronRight] = Define("chevron-right", "material-symbols/navigation/chevron-right.svg", PixelIconCategory.Navigation, "Next"),
                [PixelIconName.Library] = Define("book", "material-symbols/books/book.svg", PixelIconCategory.Navigation, "Library"),
                [PixelIconName.Daily] = Define("daily", "material-symbols/navigation/daily.svg", PixelIconCategory.Navigation, "Daily"),
                [PixelIconName.Home] = Define("home", "material-symbols/navigation/home.svg", PixelIconCategory.Navigation, "Home"),
                [PixelIconName.Profile] = Define("profile", "material-symbols/profile/profile.svg", PixelIconCategory.Navigation, "Profile"),
                [PixelIconName.Donate] = Define("donate", "material-symbols/navigation/donate.svg", PixelIconCategory.Navigation, "Donate"),
                [PixelIconName.Logout] = Define("logout", "material-symbols/navigation/logout.svg", PixelIconCategory.Navigation, "Logout"),
                [PixelIconName.Menu] = Define("menu", "material-symbols/navigation/menu.svg", PixelIconCategory.Navigation, "Menu"),
                [PixelIconName.Support] = Define("support", "material-symbols/navigation/support.svg", PixelIconCategory.Navigation, "Support"),
                [PixelIconName.Facebook] = Define("facebook", "devicon/social/facebook.svg", PixelIconCategory.Social, "Facebook"),
                [PixelIconName.Instagram] = Define("instagram", "official-brand/social/instagram.svg", PixelIconCategory.Social, "Instagram"),
                [PixelIconName.YouTube] = Define("youtube", "official-brand/social/youtube.svg", PixelIconCategory.Social, "YouTube"),
                [PixelIconName.X] = Define("x", "official-brand/social/x.svg", PixelIconCategory.Social, "X"),
                [PixelIconName.LinkedIn] = Define("linkedin", "devicon/social/linkedin.svg", PixelIconCategory.Social, "LinkedIn"),
                [PixelIconName.GitHub] = Define("github", "devicon/social/github.svg", PixelIconCategory.Social, "GitHub"),
                [PixelIconName.Habit] = Define("habit", "material-symbols/habits/habit.svg", PixelIconCategory.Activities, "Habit"),
                [PixelIconName.RecurringTask] = Define("recurring-task", "material-symbols/tasks/recurring-task.svg", PixelIconCategory.Activities, "Recurring task"),
                [PixelIconName.Project] = Define("project", "material-symbols/projects/project.svg", PixelIconCategory.Activities, "Project"),
                [PixelIconName.Todo] = Define("todo", "material-symbols/tasks/todo.svg", PixelIconCategory.Activities, "To-Do"),
                [PixelIconName.Complete] = Define("complete", "material-symbols/actions/complete.svg", PixelIconCategory.Actions, "Complete"),
                [PixelIconName.Filter] = Define("filter", "material-symbols/actions/filter.svg", PixelIconCategory.Actions, "Filter"),
                [PixelIconName.Calendar] = Define("calendar", "material-symbols/actions/calendar.svg", PixelIconCategory.Actions, "Calendar"),
                [PixelIconName.Repeat] = Define("repeat", "material-symbols/actions/repeat.svg", PixelIconCategory.Actions, "Repeat"),
                [PixelIconName.Tag] = Define("tag", "material-symbols/actions/tag.svg", PixelIconCategory.Actions, "Tag"),
                [PixelIconName.Attribute] = Define("attribute", "material-symbols/actions/attribute.svg", PixelIconCategory.Actions, "Attribute"),
                [PixelIconName.Cancel] = Define("cancel", "material-symbols/actions/cancel.svg", PixelIconCategory.Actions, "Cancel"),
                [PixelIconName.Featured] = Define("featured", "material-symbols/feedback/featured.svg", PixelIconCategory.Feedback, "Featured"),
                [PixelIconName.Progress] = Define("progress", "material-symbols/statistics/progress.svg", PixelIconCategory.Statistics, "Progress"),
                [PixelIconName.Success] = Define("success", "material-symbols/feedback/success.svg", PixelIconCategory.Feedback, "Success"),
                [PixelIconName.ValidationError] = Define("validation-error", "material-symbols/feedback/validation-error.svg", PixelIconCategory.Feedback, "Validation error"),
                [PixelIconName.Loading] = Define("loading", "material-symbols/feedback/loading.svg", PixelIconCategory.Feedback, "Loading"),
                [PixelIconName.Select] = Define("select", "material-symbols/forms/select.svg", PixelIconCategory.Forms, "Select"),
                [PixelIconName.CheckboxUnchecked] = Define("checkbox-unchecked", "material-symbols/forms/checkbox-unchecked.svg", PixelIconCategory.Forms, "Unchecked"),
                [PixelIconName.CheckboxChecked] = Define("checkbox-checked", "material-symbols/forms/checkbox-checked.svg", PixelIconCategory.Forms, "Checked"),
                [PixelIconName.Experience] = Define("experience", "material-symbols/statistics/experience.svg", PixelIconCategory.Statistics, "Experience"),
                [PixelIconName.Level] = Define("level", "material-symbols/statistics/level.svg", PixelIconCategory.Statistics, "Level"),
                [PixelIconName.Wallet] = Define("wallet", "material-symbols/statistics/wallet.svg", PixelIconCategory.Statistics, "Wallet"),
                [PixelIconName.Income] = Define("income", "material-symbols/statistics/income.svg", PixelIconCategory.Statistics, "Income"),
                [PixelIconName.Expense] = Define("expense", "material-symbols/statistics/expense.svg", PixelIconCategory.Statistics, "Expense"),
                [PixelIconName.Statistics] = Define("statistics", "material-symbols/statistics/statistics.svg", PixelIconCategory.Statistics, "Statistics"),
                [PixelIconName.TrendUp] = Define("trend-up", "material-symbols/statistics/trend-up.svg", PixelIconCategory.Statistics, "Upward trend"),
                [PixelIconName.TrendDown] = Define("trend-down", "material-symbols/statistics/trend-down.svg", PixelIconCategory.Statistics, "Downward trend"),
                [PixelIconName.Streak] = Define("streak", "material-symbols/statistics/streak.svg", PixelIconCategory.Statistics, "Streak"),
                [PixelIconName.Completed] = Define("completed", "material-symbols/statistics/completed.svg", PixelIconCategory.Statistics, "Completed"),
                [PixelIconName.Pending] = Define("pending", "material-symbols/statistics/pending.svg", PixelIconCategory.Statistics, "Pending")
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
        new(symbolId, $"/icons/{assetPath}", category, semanticName, semanticName, DefaultFallback);
}
