using LevelUp.Models;
using LevelUp.Services;

namespace LevelUp.UI
{
    public class ConsoleMenu
    {

        private readonly HabitService habitService;
        private readonly SaveService saveService;
        private readonly Character character;
        private readonly CharacterScreen characterScreen;
        private readonly TrainingScreen trainingScreen;

        public ConsoleMenu(

            HabitService habitService,
            SaveService saveService,
            Character character,
            CharacterScreen characterScreen,
            TrainingScreen trainingScreen)
        {

            this.habitService = habitService;
            this.saveService = saveService;
            this.character = character;
            this.characterScreen = characterScreen;
            this.trainingScreen = trainingScreen;
        }

        public void Start()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();

                ShowMainMenu();

                string? option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        characterScreen.Show(character);
                        break;

                    case "2":
                        trainingScreen.Show();
                        break;

                    case "0":
                        SaveGame();
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }

                if (running)
                {
                    Console.WriteLine();
                    Console.WriteLine("Pressione qualquer tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }

        private void ShowMainMenu()
        {
            Console.WriteLine("================================");
            Console.WriteLine("           LEVEL UP");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("1 - Ver personagem");
            Console.WriteLine("2 - Training");
            Console.WriteLine("0 - Sair");
            Console.WriteLine();
            Console.Write("Escolha uma opção: ");
        }

        private void SaveGame()
        {
            GameData gameData = new GameData
            {
                Character = character,
                Habits = habitService.GetAllHabits()
            };

            saveService.SaveGame(gameData);

            Console.WriteLine();
            Console.WriteLine("Progresso salvo com sucesso.");
        }
    }
}