using LevelUp.Models;
using LevelUp.Services;

namespace LevelUp.UI
{
    public class ConsoleMenu
    {
        private readonly CharacterService characterService;
        private readonly HabitService habitService;
        private readonly SaveService saveService;
        private readonly Character character;
        private readonly CharacterScreen characterScreen;
        private readonly AttributeService attributeService;

        public ConsoleMenu(
            CharacterService characterService,
            HabitService habitService,
            SaveService saveService,
            Character character,
            CharacterScreen characterScreen,
            AttributeService attributeService)
        {
            this.characterService = characterService;
            this.habitService = habitService;
            this.saveService = saveService;
            this.character = character;
            this.characterScreen = characterScreen;
            this.attributeService = attributeService;
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

            AttributeType attributeType = SelectAttribute();

            Habit habit = habitService.CreateHabit(
                title,
                description,
                durationInMinutes,
                attributeType

            );

            Console.WriteLine();
            Console.WriteLine("Hábito cadastrado com sucesso.");
            Console.WriteLine($"ID         : {habit.Id}");
            Console.WriteLine($"Título     : {habit.Title}");
            Console.WriteLine($"Recompensa : {habit.ExperienceReward} XP");
        }

        private AttributeType SelectAttribute()
        {
            Console.WriteLine();
            Console.WriteLine("Escolha o atributo deste hábito:");
            Console.WriteLine("1 - Strength");
            Console.WriteLine("2 - Intelligence");
            Console.WriteLine("3 - Vitality");
            Console.WriteLine("4 - Agility");
            Console.WriteLine("5 - Luck");
            Console.WriteLine("6 - Dexterity");
            Console.WriteLine();

            Console.Write("Opção: ");

            bool validOption = int.TryParse(
                Console.ReadLine(),
                out int option
            );

            if (!validOption ||
                !Enum.IsDefined(typeof(AttributeType), option))
            {
                Console.WriteLine("Atributo inválido.");

                return SelectAttribute();
            }

            return (AttributeType)option;
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

            attributeService.AddExperience(
                character.Attributes,
                selectedHabit.AttributeType,
                selectedHabit.AttributeExperienceReward
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