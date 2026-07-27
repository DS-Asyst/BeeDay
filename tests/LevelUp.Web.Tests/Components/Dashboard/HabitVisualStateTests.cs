using LevelUp.Web.Components.Features.Habits;

namespace LevelUp.Web.Tests.Components.Dashboard;

public sealed class HabitVisualStateTests
{
    [Theory]
    [InlineData(0, "habit-card--white", "habit-editor--white")]
    [InlineData(7, "habit-card--yellow", "habit-editor--yellow")]
    [InlineData(14, "habit-card--green", "habit-editor--green")]
    [InlineData(21, "habit-card--sky", "habit-editor--sky")]
    [InlineData(-1, "habit-card--red-light", "habit-editor--red-light")]
    [InlineData(-7, "habit-card--red-medium", "habit-editor--red-medium")]
    [InlineData(-14, "habit-card--red-strong", "habit-editor--red-strong")]
    public void ResolvesMatchingCardAndEditorClasses(int balance, string cardClass, string editorClass)
    {
        Assert.Equal(cardClass, HabitVisualState.GetCardClass(balance));
        Assert.Equal(editorClass, HabitVisualState.GetEditorClass(balance));
    }
}
