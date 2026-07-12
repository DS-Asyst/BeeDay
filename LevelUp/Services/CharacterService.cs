using LevelUp.Models;

namespace LevelUp.Services
{
    public class CharacterService
    {
        public Character CreateCharacter(string name)
        {
            Character character = new Character
            {
                Name = name
            };

            return character;
        }

        public void AddExperience(Character character, decimal experienceEarned)
        {
            character.Experience += experienceEarned;

            while (character.Experience >= character.ExperienceToNextLevel)
            {
                decimal requiredExperience = character.ExperienceToNextLevel;

                character.Experience -= requiredExperience;
                character.Level++;
            }
        }
    }
}