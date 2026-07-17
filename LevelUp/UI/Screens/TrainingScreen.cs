using LevelUp.Domain.Attributes;
using LevelUp.Domain.Habits;
using LevelUp.Domain.Rewards;
using LevelUp.Services.Character;
using LevelUp.Services.Habits;
using LevelUp.Services.Persistence;
using LevelUp.UI.Components.Training;
using LevelUp.UI.Infrastructure;
using Spectre.Console;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI;

public class TrainingScreen
{
    private readonly HabitService habitService;
    private readonly CharacterService characterService;
    private readonly AttributeService attributeService;
    private readonly InputReader inputReader;
    private readonly CharacterModel character;
    private readonly GameStateService gameStateService;

    public TrainingScreen(
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
            ConsoleHelper.ShowHeader("Painel de Treinamentos");

            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                    "Novo treinamento",
                    "Abrir treinamento",
                    "Listar treinamentos",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Novo treinamento":
                    CreateTraining();
                    inputReader.WaitForContinue();
                    break;

                case "Abrir treinamento":
                    OpenTraining();
                    break;

                case "Listar treinamentos":
                    ListTrainings();
                    inputReader.WaitForContinue();
                    break;

                case "Voltar":
                    running = false;
                    break;
            }
        }
    }

    private void CreateTraining()
    {
        ConsoleHelper.ShowHeader("Novo treinamento");
        inputReader.ShowCancellationHint();

        try
        {
            string title = inputReader.ReadRequiredStringOrCancel(
                "Título:"
            );
            string description = inputReader.ReadRequiredStringOrCancel(
                "Descrição:"
            );
            AttributeType attributeType = SelectAttribute(
                includeCancellation: true
            );

            Habit habit = habitService.CreateHabit(
                title,
                description,
                attributeType
            );

            gameStateService.Save();
            ConsoleHelper.ShowSuccess(
                "Treinamento cadastrado com sucesso."
            );
            AnsiConsole.WriteLine();
            AnsiConsole.Write(
                new TrainingCreatedCard(habit).Build()
            );
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation(
                "Criação do treinamento cancelada."
            );
        }
    }

    private void OpenTraining()
    {
        List<Habit> habits = habitService.GetAllHabits();

        if (habits.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum treinamento foi cadastrado."
            );
            inputReader.WaitForContinue();
            return;
        }

        Habit habit = SelectTraining(
            "Selecione um treinamento:",
            habits
        );
        bool opened = true;

        while (opened)
        {
            ConsoleHelper.ShowHeader("Treinamento");
            AnsiConsole.Write(
                new TrainingCreatedCard(habit).Build()
            );
            AnsiConsole.WriteLine();

            string action = inputReader.ReadSelection(
                "Escolha uma ação:",
                new[]
                {
                    "Editar",
                    "Concluir",
                    "Excluir",
                    "Voltar"
                },
                choice => choice
            );

            switch (action)
            {
                case "Editar":
                    EditTraining(habit);
                    inputReader.WaitForContinue();
                    break;

                case "Concluir":
                    CompleteTraining(habit);
                    inputReader.WaitForContinue();
                    break;

                case "Excluir":
                    opened = !DeleteTraining(habit);
                    if (opened)
                    {
                        inputReader.WaitForContinue();
                    }
                    break;

                case "Voltar":
                    opened = false;
                    break;
            }
        }
    }

    private void ListTrainings()
    {
        ConsoleHelper.ShowHeader("Treinamentos");

        List<Habit> habits = habitService.GetAllHabits();

        if (habits.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum treinamento foi cadastrado."
            );
            return;
        }

        AnsiConsole.Write(
            new TrainingTable(habits).Build()
        );
    }

    private void EditTraining(Habit habit)
    {
        inputReader.ShowCancellationHint();

        try
        {
            AnsiConsole.MarkupLine(
                $"[grey]Título atual:[/] {Markup.Escape(habit.Title)}"
            );
            string title = inputReader.ReadRequiredStringOrCancel(
                "Novo título:"
            );

            AnsiConsole.MarkupLine(
                $"[grey]Descrição atual:[/] " +
                Markup.Escape(habit.Description)
            );
            string description = inputReader.ReadRequiredStringOrCancel(
                "Nova descrição:"
            );

            AttributeType attributeType = SelectAttribute(
                includeCancellation: true
            );

            if (!inputReader.ReadConfirmation(
                $"Salvar alterações em '{habit.Title}'?"
            ))
            {
                ConsoleHelper.ShowInformation(
                    "Edição cancelada."
                );
                return;
            }

            habitService.UpdateHabit(
                habit,
                title,
                description,
                attributeType
            );
            gameStateService.Save();
            ConsoleHelper.ShowSuccess(
                "Treinamento atualizado com sucesso."
            );
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation(
                "Edição do treinamento cancelada."
            );
        }
    }

    private void CompleteTraining(Habit habit)
    {
        if (!inputReader.ReadConfirmation(
            $"Concluir o treinamento '{habit.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Conclusão do treinamento cancelada."
            );
            return;
        }

        Reward reward = habitService.CompleteHabit(habit);
        character.ApplyReward(reward);
        decimal experienceEarned = reward.Experience;

        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Treinamento concluído com sucesso."
        );
        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new TrainingResultCard(
                habit,
                character,
                experienceEarned
            ).Build()
        );
    }

    private bool DeleteTraining(Habit habit)
    {
        if (!inputReader.ReadConfirmation(
            $"Excluir permanentemente '{habit.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Exclusão cancelada."
            );
            return false;
        }

        if (!habitService.DeleteHabit(habit.Id))
        {
            ConsoleHelper.ShowError(
                "Não foi possível excluir o treinamento."
            );
            return false;
        }

        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Treinamento excluído com sucesso."
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
            choices.Add("Cancelar");
        }

        string selected = inputReader.ReadSelection(
            "Escolha o atributo:",
            choices,
            choice => choice
        );

        if (selected == "Cancelar")
        {
            throw new UserCancelledException();
        }

        return Enum
            .GetValues<AttributeType>()
            .First(attribute => DisplayText.For(attribute) == selected);
    }

    private Habit SelectTraining(
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
