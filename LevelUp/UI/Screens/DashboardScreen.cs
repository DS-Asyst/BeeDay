using LevelUp.Services.Analytics;
using LevelUp.UI.Infrastructure;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class DashboardScreen
{
    private readonly DashboardService dashboardService;
    private readonly InputReader inputReader;

    public DashboardScreen(DashboardService dashboardService, InputReader inputReader)
    {
        this.dashboardService = dashboardService;
        this.inputReader = inputReader;
    }

    public void Show()
    {
        ConsoleHelper.ShowHeader("Visão geral");
        DashboardSnapshot snapshot = dashboardService.GetSnapshot(DateTime.Now);

        Table table = new Table().Border(TableBorder.Rounded).Expand();
        table.AddColumn("Área");
        table.AddColumn("Indicador");
        table.AddColumn("Valor");

        table.AddRow("Personagem", "Nível / Título", $"{snapshot.Level} / {DisplayText.For(snapshot.Rank)}");
        table.AddRow("Personagem", "Experiência", $"{snapshot.Experience:0.##} / {snapshot.ExperienceToNextLevel:0.##}");
        table.AddRow("Diário", "Projetos ativos", snapshot.ActiveProjects.ToString());
        table.AddRow("Diário", "Missões ativas", snapshot.ActiveQuests.ToString());
        table.AddRow("Diário", "Missões concluídas", snapshot.CompletedQuests.ToString());
        table.AddRow("Biblioteca", "Livros em andamento", snapshot.ActiveBooks.ToString());
        table.AddRow("Biblioteca", "Páginas no mês", snapshot.PagesReadThisMonth.ToString());
        table.AddRow("Carteira", "Saldo", $"R$ {snapshot.WalletBalance:N2}");
        table.AddRow("Carteira", "Resultado do mês", $"R$ {snapshot.WalletMonthResult:N2}");
        table.AddRow("Reconhecimento", "Conquistas", snapshot.UnlockedAchievements.ToString());

        AnsiConsole.Write(table);
        inputReader.WaitForContinue();
    }
}
