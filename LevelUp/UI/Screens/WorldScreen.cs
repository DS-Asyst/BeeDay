using LevelUp.Application;
using LevelUp.Domain.Goals;
using LevelUp.Services.Goals;
using LevelUp.Services.Persistence;
using LevelUp.UI.Infrastructure;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class WorldScreen
{
    private readonly GoalService goalService;
    private readonly GameSession session;
    private readonly GameStateService gameStateService;
    private readonly InputReader inputReader;

    public WorldScreen(
        GoalService goalService,
        GameSession session,
        GameStateService gameStateService,
        InputReader inputReader
    )
    {
        this.goalService = goalService;
        this.session = session;
        this.gameStateService = gameStateService;
        this.inputReader = inputReader;
    }

    public void Show()
    {
        bool running = true;
        while (running)
        {
            goalService.EvaluateAll(session);
            ConsoleHelper.ShowHeader("Mundo");
            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[] { "Metas", "Criar meta", "Arquivar meta", "Excluir meta", "Voltar" },
                choice => choice
            );

            try
            {
                switch (option)
                {
                    case "Metas": ShowGoals(); inputReader.WaitForContinue(); break;
                    case "Criar meta": CreateGoal(); break;
                    case "Arquivar meta": ArchiveGoal(); break;
                    case "Excluir meta": DeleteGoal(); break;
                    case "Voltar": running = false; break;
                }
            }
            catch (OperationCanceledException)
            {
                ConsoleHelper.ShowInformation("Operação cancelada.");
                inputReader.WaitForContinue();
            }
        }
    }

    private void ShowGoals()
    {
        ConsoleHelper.ShowHeader("Metas e desafios");
        IReadOnlyList<Goal> goals = goalService.GetAll();
        if (goals.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhuma meta foi cadastrada.");
            return;
        }

        Table table = new Table().Border(TableBorder.Rounded).Expand();
        table.AddColumn("ID");
        table.AddColumn("Meta");
        table.AddColumn("Indicador");
        table.AddColumn("Progresso");
        table.AddColumn("Status");
        foreach (Goal goal in goals)
        {
            decimal current = goalService.GetCurrentValue(goal, session);
            decimal percent = Math.Min(100m, current * 100m / goal.TargetValue);
            table.AddRow(
                goal.Id.ToString(),
                Markup.Escape(goal.Name),
                MetricText(goal.Metric),
                $"{current:0.##} / {goal.TargetValue:0.##} ({percent:0.##}%)",
                StatusText(goal.Status)
            );
        }
        AnsiConsole.Write(table);
    }

    private void CreateGoal()
    {
        ConsoleHelper.ShowHeader("Nova meta");
        inputReader.ShowCancellationHint();
        string name = inputReader.ReadRequiredStringOrCancel("Nome:");
        string description = inputReader.ReadRequiredStringOrCancel("Descrição:");
        GoalMetric metric = inputReader.ReadSelection(
            "Indicador:",
            Enum.GetValues<GoalMetric>(),
            MetricText
        );
        decimal target = inputReader.ReadPositiveDecimalOrCancel("Valor-alvo:");
        goalService.Create(name, description, metric, target);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Meta criada com sucesso.");
        inputReader.WaitForContinue();
    }

    private void ArchiveGoal()
    {
        Goal? goal = SelectGoal("Selecione a meta para arquivar:");
        if (goal is null) return;
        goal.Archive();
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Meta arquivada.");
        inputReader.WaitForContinue();
    }

    private void DeleteGoal()
    {
        Goal? goal = SelectGoal("Selecione a meta para excluir:");
        if (goal is null) return;
        if (!inputReader.ReadConfirmation("Confirma a exclusão da meta?")) return;
        goalService.Delete(goal.Id);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Meta excluída.");
        inputReader.WaitForContinue();
    }

    private Goal? SelectGoal(string prompt)
    {
        IReadOnlyList<Goal> goals = goalService.GetAll();
        if (goals.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhuma meta foi cadastrada.");
            inputReader.WaitForContinue();
            return null;
        }
        return inputReader.ReadSelection(prompt, goals, goal => $"{goal.Name} — {StatusText(goal.Status)}");
    }

    private static string MetricText(GoalMetric metric) => metric switch
    {
        GoalMetric.CompletedQuests => "Missões concluídas",
        GoalMetric.CompletedTrainings => "Treinamentos concluídos",
        GoalMetric.CompletedProjects => "Projetos concluídos",
        GoalMetric.CompletedBooks => "Livros concluídos",
        GoalMetric.PagesRead => "Páginas lidas",
        GoalMetric.SavingsBalance => "Saldo da carteira",
        _ => metric.ToString()
    };

    private static string StatusText(GoalStatus status) => status switch
    {
        GoalStatus.Active => "Ativa",
        GoalStatus.Completed => "Concluída",
        GoalStatus.Archived => "Arquivada",
        _ => status.ToString()
    };
}
