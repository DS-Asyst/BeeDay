using System.Text.Json.Nodes;

namespace LevelUp.Infrastructure.Persistence.Json;

internal static class LegacyActivityAttributeMigrator
{
    private static readonly string[] RemovedAttributeNames = ["Wisdom", "Charisma"];

    public static void Migrate(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                foreach (var property in jsonObject.ToList())
                {
                    if (string.Equals(property.Key, "attribute", StringComparison.OrdinalIgnoreCase)
                        && property.Value is JsonValue value
                        && value.TryGetValue<string>(out var attributeName)
                        && RemovedAttributeNames.Contains(attributeName, StringComparer.OrdinalIgnoreCase))
                    {
                        jsonObject[property.Key] = null;
                        continue;
                    }

                    Migrate(property.Value);
                }
                break;

            case JsonArray jsonArray:
                foreach (var item in jsonArray)
                {
                    Migrate(item);
                }
                break;
        }
    }
}
