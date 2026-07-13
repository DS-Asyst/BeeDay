using LevelUp.Models;

namespace LevelUp.Services
{
    public class CharacterService
    {
        private readonly ProgressionService progressionService;

        public CharacterService(
            ProgressionService progressionService)
        {
            this.progressionService = progressionService;
        }
        public Character CreateCharacter(string name)
        {
            Character character = new Character
            {
                Name = name
            };

            return character;
        }

        public void AddExperience(
            Character character,
            decimal experienceEarned)
        {
            progressionService.AddExperience(
                character,
                experienceEarned
            );
        }
    }
}