namespace LevelUp.UI;

public sealed class DiaryScreen
{
    private readonly InputReader inputReader;
    private readonly TrainingScreen trainingScreen;
    private readonly QuestScreen questScreen;
    private readonly ProjectScreen projectScreen;
    private readonly MilestoneScreen milestoneScreen;

    public DiaryScreen(
        InputReader inputReader,
        TrainingScreen trainingScreen,
        QuestScreen questScreen,
        ProjectScreen projectScreen,
        MilestoneScreen milestoneScreen
    )
    {
        this.inputReader = inputReader;
        this.trainingScreen = trainingScreen;
        this.questScreen = questScreen;
        this.projectScreen = projectScreen;
        this.milestoneScreen = milestoneScreen;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Diário");

            string option = inputReader.ReadSelection(
                "Escolha uma seção:",
                new[]
                {
                    "Treinamentos",
                    "Missões",
                    "Projetos",
                    "Capítulos",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Treinamentos":
                    trainingScreen.Show();
                    break;

                case "Missões":
                    questScreen.Show();
                    break;

                case "Projetos":
                    projectScreen.Show();
                    break;

                case "Capítulos":
                    milestoneScreen.Show();
                    break;

                case "Voltar":
                    running = false;
                    break;
            }
        }
    }
}
