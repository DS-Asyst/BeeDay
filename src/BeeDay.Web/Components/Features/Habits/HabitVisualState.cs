namespace BeeDay.Web.Components.Features.Habits;

public static class HabitVisualState
{
    public static string GetModifier(int balance) => balance switch
    {
        >= 21 => "sky",
        >= 14 => "green",
        >= 7 => "yellow",
        <= -14 => "red-strong",
        <= -7 => "red-medium",
        <= -1 => "red-light",
        _ => "white"
    };

    public static string GetCardClass(int balance) => $"habit-card--{GetModifier(balance)}";
    public static string GetEditorClass(int balance) => $"habit-editor--{GetModifier(balance)}";
}
