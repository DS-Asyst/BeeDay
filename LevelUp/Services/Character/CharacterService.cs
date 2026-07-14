using LevelUp.Domain.Character;
using CharacterModel = LevelUp.Domain.Character.Character;

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
            CharacterModel character = new Character
            {
                Name = name
            };

            return character;
        }

        public void AddExperience(
            CharacterModel character,
            decimal experienceEarned)
        {
            progressionService.AddExperience(
                character,
                experienceEarned
            );
        }
    }
}