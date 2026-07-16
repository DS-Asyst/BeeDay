namespace LevelUp.UI;

public sealed class DiaryScreen
{
    private readonly InputReader inputReader;
    private readonly TrainingScreen trainingScreen;
    private readonly QuestScreen questScreen;
    private readonly ProjectScreen projectScreen;

    public DiaryScreen(
        InputReader inputReader,
        TrainingScreen trainingScreen,
        QuestScreen questScreen,
        ProjectScreen projectScreen
    )
    {
        this.inputReader = inputReader;
        this.trainingScreen = trainingScreen;
        this.questScreen = questScreen;
        this.projectScreen = projectScreen;
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

                case "Voltar":
                    running = false;
                    break;
            }
        }
    }
}
