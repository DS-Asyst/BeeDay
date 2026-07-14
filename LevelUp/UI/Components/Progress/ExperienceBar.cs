using LevelUp.UI.Themes;

namespace LevelUp.UI.Components;

public static class ExperienceBar
{
    public static string Render(
        decimal currentValue,
        decimal maximumValue,
        int width = 30
    )
    {
        decimal percentage = CalculatePercentage(
            currentValue,
            maximumValue
        );

        int completedBlocks = (int)Math.Round(
            percentage / 100 * width
        );

        completedBlocks = Math.Clamp(
            completedBlocks,
            0,
            width
        );

        int remainingBlocks = width - completedBlocks;

        string completed = new('█', completedBlocks);
        string remaining = new('░', remainingBlocks);

        return
            $"[{LevelUpTheme.Experience}]{completed}[/]" +
            $"[{LevelUpTheme.MutedText}]{remaining}[/] " +
            $"[{LevelUpTheme.Primary}]{percentage:0}%[/]";
    }

    private static decimal CalculatePercentage(
        decimal currentValue,
        decimal maximumValue
    )
    {
        if (maximumValue <= 0)
        {
            return 0;
        }

        decimal percentage =
            currentValue / maximumValue * 100;

        return Math.Clamp(percentage, 0, 100);
    }
}