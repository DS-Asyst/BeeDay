using LevelUp.Models;
using LevelUp.Services;

namespace LevelUp.UI
{
    public class ConsoleMenu
    {
        private readonly CharacterService characterService;
        private readonly HabitService habitService;
        private readonly Character character;
        private readonly SaveService saveService;

        public ConsoleMenu(
            CharacterService characterService,
            HabitService habitService,
            SaveService saveService,
            Character character)
        {
            this.characterService = characterService;
            this.habitService = habitService;
            this.character = character;
            this.saveService = saveService;
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
                        ShowCharacter();
                        break;

                    case "2":
                        CreateHabit();
                        break;

                    case "3":
                        ListHabits();
                        break;

                    case "4":
                        CompleteHabit();
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
            Console.WriteLine("2 - Cadastrar hábito");
            Console.WriteLine("3 - Listar hábitos");
            Console.WriteLine("4 - Concluir hábito");
            Console.WriteLine("0 - Sair");
            Console.WriteLine();
            Console.Write("Escolha uma opção: ");
        }

        private void ShowCharacter()
        {
            Console.Clear();

            Console.WriteLine("===== PERSONAGEM =====");
            Console.WriteLine($"Nome        : {character.Name}");
            Console.WriteLine($"Nível       : {character.Level}");
            Console.WriteLine(
                $"Experiência : {character.Experience}/{character.ExperienceToNextLevel}"
            );
        }

        private void CreateHabit()
        {
            Console.Clear();

            Console.WriteLine("===== CADASTRAR HÁBITO =====");
            Console.WriteLine();

            Console.Write("Título: ");
            string title = Console.ReadLine() ?? string.Empty;

            Console.Write("Descrição: ");
            string description = Console.ReadLine() ?? string.Empty;

            Console.Write("Duração em minutos: ");
            bool validDuration = int.TryParse(
                Console.ReadLine(),
                out int durationInMinutes
            );

            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("O título não pode ficar vazio.");
                return;
            }

            if (!validDuration || durationInMinutes <= 0)
            {
                Console.WriteLine("A duração deve ser maior que zero.");
                return;
            }

            Habit habit = habitService.CreateHabit(
                title,
                description,
                durationInMinutes
            );

            Console.WriteLine();
            Console.WriteLine("Hábito cadastrado com sucesso.");
            Console.WriteLine($"ID         : {habit.Id}");
            Console.WriteLine($"Título     : {habit.Title}");
            Console.WriteLine($"Recompensa : {habit.ExperienceReward} XP");
        }

        private void ListHabits()
        {
            Console.Clear();

            Console.WriteLine("===== HÁBITOS CADASTRADOS =====");
            Console.WriteLine();

            List<Habit> habits = habitService.GetAllHabits();

            if (habits.Count == 0)
            {
                Console.WriteLine("Nenhum hábito foi cadastrado.");
                return;
            }

            foreach (Habit habit in habits)
            {
                Console.WriteLine($"ID         : {habit.Id}");
                Console.WriteLine($"Título     : {habit.Title}");
                Console.WriteLine($"Descrição  : {habit.Description}");
                Console.WriteLine($"Duração    : {habit.DurationInMinutes} minutos");
                Console.WriteLine($"Recompensa : {habit.ExperienceReward} XP");
                Console.WriteLine($"Conclusões : {habit.TimesCompleted}");
                Console.WriteLine("-------------------------------");
            }
        }

        private void CompleteHabit()
        {
            Console.Clear();

            Console.WriteLine("===== CONCLUIR HÁBITO =====");
            Console.WriteLine();

            List<Habit> habits = habitService.GetAllHabits();

            if (habits.Count == 0)
            {
                Console.WriteLine("Nenhum hábito foi cadastrado.");
                return;
            }

            foreach (Habit habit in habits)
            {
                Console.WriteLine(
                    $"{habit.Id} - {habit.Title} " +
                    $"({habit.ExperienceReward} XP)"
                );
            }

            Console.WriteLine();
            Console.Write("Digite o ID do hábito concluído: ");

            bool validId = int.TryParse(
                Console.ReadLine(),
                out int habitId
            );

            if (!validId)
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            Habit? selectedHabit = habits.FirstOrDefault(
                habit => habit.Id == habitId
            );

            if (selectedHabit is null)
            {
                Console.WriteLine("Hábito não encontrado.");
                return;
            }

            decimal experienceEarned =
                habitService.CompleteHabit(selectedHabit);

            characterService.AddExperience(
                character,
                experienceEarned
            );

            Console.WriteLine();
            Console.WriteLine("Hábito concluído com sucesso.");
            Console.WriteLine($"Hábito      : {selectedHabit.Title}");
            Console.WriteLine($"XP recebida : {experienceEarned}");
            Console.WriteLine(
                $"Conclusões  : {selectedHabit.TimesCompleted}"
            );

            Console.WriteLine();
            Console.WriteLine($"Nível atual : {character.Level}");
            Console.WriteLine(
                $"Experiência : {character.Experience}/" +
                $"{character.ExperienceToNextLevel}"
            );
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