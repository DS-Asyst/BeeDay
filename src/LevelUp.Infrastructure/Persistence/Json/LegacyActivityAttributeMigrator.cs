using System.Text.Json.Nodes;

namespace LevelUp.Infrastructure.Persistence.Json;

/// <summary>
/// Strips "Wisdom" and "Charisma" values from any "attribute" field in legacy documents.
/// These were remnants of the former RPG ability-score system; the current
/// <see cref="LevelUp.Domain.Enums.ActivityAttribute"/> classifier never includes them.
/// No-op on documents that never had these values.
/// </summary>
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
