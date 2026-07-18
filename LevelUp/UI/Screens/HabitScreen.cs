using LevelUp.Domain.Attributes;
using LevelUp.Domain.Habits;
using LevelUp.Domain.Rewards;
using LevelUp.Services.Character;
using LevelUp.Services.Habits;
using LevelUp.Services.Persistence;
using LevelUp.UI.Components.Habit;
using LevelUp.UI.Infrastructure;
using Spectre.Console;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI;

public class HabitScreen
{
    private readonly HabitService habitService;
    private readonly CharacterService characterService;
    private readonly AttributeService attributeService;
    private readonly InputReader inputReader;
    private readonly CharacterModel character;
    private readonly GameStateService gameStateService;

    public HabitScreen(
        HabitService habitService,
        CharacterService characterService,
        AttributeService attributeService,
        InputReader inputReader,
        CharacterModel character,
        GameStateService gameStateService
    )
    {
        this.habitService = habitService;
        this.characterService = characterService;
        this.attributeService = attributeService;
        this.inputReader = inputReader;
        this.character = character;
        this.gameStateService = gameStateService;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Habits");

            string option = inputReader.ReadSelection(
                "Choose an option:",
                new[]
                {
                    "New Habit",
                    "Open Habit",
                    "List Habits",
                    "Back"
                },
                choice => choice
            );

            switch (option)
            {
                case "New Habit":
                    CreateHabit();
                    inputReader.WaitForContinue();
                    break;

                case "Open Habit":
                    OpenHabit();
                    break;

                case "List Habits":
                    ListHabits();
                    inputReader.WaitForContinue();
                    break;

                case "Back":
                    running = false;
                    break;
            }
        }
    }

    private void CreateHabit()
    {
        ConsoleHelper.ShowHeader("New Habit");
        inputReader.ShowCancellationHint();

        try
        {
            string title = inputReader.ReadRequiredStringOrCancel(
                "Title:"
            );
            string description = inputReader.ReadRequiredStringOrCancel(
                "Description:"
            );
            AttributeType attributeType = SelectAttribute(
                includeCancellation: true
            );
            HabitDirection direction = SelectDirection();

            Habit habit = habitService.CreateHabit(
                title,
                description,
                attributeType,
                direction
            );

            gameStateService.Save();
            ConsoleHelper.ShowSuccess(
                "Habit created successfully."
            );
            AnsiConsole.WriteLine();
            AnsiConsole.Write(
                new HabitCreatedCard(habit).Build()
            );
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation(
                "Habit creation cancelled."
            );
        }
    }

    private void OpenHabit()
    {
        IReadOnlyList<Habit> habits = habitService.GetAllHabits();

        if (habits.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "No habits have been created."
            );
            inputReader.WaitForContinue();
            return;
        }

        Habit habit = SelectHabit(
            "Select a habit:",
            habits
        );
        bool opened = true;

        while (opened)
        {
            ConsoleHelper.ShowHeader("Habit");
            AnsiConsole.Write(
                new HabitCreatedCard(habit).Build()
            );
            AnsiConsole.WriteLine();

            string action = inputReader.ReadSelection(
                "Choose an action:",
                BuildHabitActions(habit),
                choice => choice
            );

            switch (action)
            {
                case "Edit":
                    EditHabit(habit);
                    inputReader.WaitForContinue();
                    break;

                case "Score Positive":
                    ScorePositive(habit);
                    inputReader.WaitForContinue();
                    break;

                case "Score Negative":
                    ScoreNegative(habit);
                    inputReader.WaitForContinue();
                    break;

                case "Delete":
                    opened = !DeleteHabit(habit);
                    if (opened)
                    {
                        inputReader.WaitForContinue();
                    }
                    break;

                case "Back":
                    opened = false;
                    break;
            }
        }
    }

    private void ListHabits()
    {
        ConsoleHelper.ShowHeader("Habits");

        IReadOnlyList<Habit> habits = habitService.GetAllHabits();

        if (habits.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "No habits have been created."
            );
            return;
        }

        AnsiConsole.Write(
            new HabitTable(habits).Build()
        );
    }

    private void EditHabit(Habit habit)
    {
        inputReader.ShowCancellationHint();

        try
        {
            AnsiConsole.MarkupLine(
                $"[grey]Current title:[/] {Markup.Escape(habit.Title)}"
            );
            string title = inputReader.ReadRequiredStringOrCancel(
                "New title:"
            );

            AnsiConsole.MarkupLine(
                $"[grey]Current description:[/] " +
                Markup.Escape(habit.Description)
            );
            string description = inputReader.ReadRequiredStringOrCancel(
                "New description:"
            );

            AttributeType attributeType = SelectAttribute(
                includeCancellation: true
            );
            HabitDirection direction = SelectDirection();

            if (!inputReader.ReadConfirmation(
                $"Save changes to '{habit.Title}'?"
            ))
            {
                ConsoleHelper.ShowInformation(
                    "Edit cancelled."
                );
                return;
            }

            habitService.UpdateHabit(
                habit,
                title,
                description,
                attributeType,
                direction
            );
            gameStateService.Save();
            ConsoleHelper.ShowSuccess(
                "Habit updated successfully."
            );
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation(
                "Habit edit cancelled."
            );
        }
    }

    private void ScorePositive(Habit habit)
    {
        if (!inputReader.ReadConfirmation(
            $"Score '{habit.Title}' positively?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Habit scoring cancelled."
            );
            return;
        }

        Reward reward = habitService.ScorePositive(habit);
        character.ApplyReward(reward);
        decimal experienceEarned = reward.Experience;

        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Habit scored successfully."
        );
        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new HabitResultCard(
                habit,
                character,
                experienceEarned
            ).Build()
        );
    }

    private void ScoreNegative(Habit habit)
    {
        if (!inputReader.ReadConfirmation($"Score '{habit.Title}' negatively?"))
        {
            ConsoleHelper.ShowInformation("Negative scoring cancelled.");
            return;
        }

        habitService.ScoreNegative(habit);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Habit scored negatively.");
    }

    private static string[] BuildHabitActions(Habit habit)
    {
        List<string> actions = ["Edit"];
        if (habit.AllowsPositive) actions.Add("Score Positive");
        if (habit.AllowsNegative) actions.Add("Score Negative");
        actions.Add("Delete");
        actions.Add("Back");
        return actions.ToArray();
    }

    private HabitDirection SelectDirection()
    {
        return inputReader.ReadSelection(
            "Direction:",
            Enum.GetValues<HabitDirection>(),
            direction => direction.ToString());
    }

    private bool DeleteHabit(Habit habit)
    {
        if (!inputReader.ReadConfirmation(
            $"Permanently delete '{habit.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Deletion cancelled."
            );
            return false;
        }

        if (!habitService.DeleteHabit(habit.Id))
        {
            ConsoleHelper.ShowError(
                "The habit could not be deleted."
            );
            return false;
        }

        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Habit deleted successfully."
        );
        return true;
    }

    private AttributeType SelectAttribute(bool includeCancellation)
    {
        List<string> choices = Enum
            .GetValues<AttributeType>()
            .Select(attribute => DisplayText.For(attribute))
            .ToList();

        if (includeCancellation)
        {
            choices.Add("Cancel");
        }

        string selected = inputReader.ReadSelection(
            "Choose an attribute:",
            choices,
            choice => choice
        );

        if (selected == "Cancel")
        {
            throw new UserCancelledException();
        }

        return Enum
            .GetValues<AttributeType>()
            .First(attribute => DisplayText.For(attribute) == selected);
    }

    private Habit SelectHabit(
        string prompt,
        IEnumerable<Habit> habits
    )
    {
        return inputReader.ReadSelection(
            prompt,
            habits,
            habit =>
                $"{habit.Title} — " +
                $"{habit.ExperienceReward:0.##} XP — " +
                DisplayText.For(habit.AttributeType)
        );
    }
}
