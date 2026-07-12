using System.Text.Json;
using LevelUp.Models;

namespace LevelUp.Services
{
    public class SaveService
    {
        private readonly string filePath;

        public SaveService()
        {
            string dataDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Data"
            );

            Directory.CreateDirectory(dataDirectory);

            filePath = Path.Combine(
                dataDirectory,
                "save.json"
            );
        }

        public void SaveGame(GameData gameData)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(
                gameData,
                options
            );

            File.WriteAllText(filePath, json);
        }

        public GameData? LoadGame()
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            string json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<GameData>(json);
        }
    }
}