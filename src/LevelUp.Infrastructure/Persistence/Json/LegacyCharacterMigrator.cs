using System.Text.Json.Nodes;

namespace LevelUp.Infrastructure.Persistence.Json;

/// <summary>
/// Reads documents written before the Character entity was merged into User (schema 5 and
/// earlier): moves each "characters[]" entry's nickname/avatar/experience onto the matching
/// "users[]" entry, re-keys each experience entry's "characterId" foreign key to "userId"
/// (the value changes from the old Character's own id to its owning User's id), and drops the
/// now-empty "characters" array. No-op on documents that already use the current shape.
/// </summary>
internal static class LegacyCharacterMigrator
{
    public static void Migrate(JsonNode? node)
    {
        if (node is not JsonObject root || root["characters"] is not JsonArray characters)
        {
            return;
        }

        var users = root["users"] as JsonArray;

        foreach (var characterNode in characters)
        {
            if (characterNode is not JsonObject character)
            {
                continue;
            }

            var userId = character["userId"]?.GetValue<string>();
            var matchingUser = users?
                .OfType<JsonObject>()
                .FirstOrDefault(user => string.Equals(user["id"]?.GetValue<string>(), userId, StringComparison.OrdinalIgnoreCase));

            if (matchingUser is null)
            {
                continue;
            }

            matchingUser["nickname"] = character["nickname"]?.DeepClone();
            matchingUser["avatar"] = character["avatar"]?.DeepClone();

            if (character["experience"] is JsonObject experience)
            {
                if (experience["Transactions"] is JsonArray entries)
                {
                    foreach (var entryNode in entries)
                    {
                        if (entryNode is JsonObject entry && entry.Remove("characterId"))
                        {
                            entry["userId"] = userId;
                        }
                    }
                }

                matchingUser["experience"] = experience.DeepClone();
            }
        }

        root.Remove("characters");
    }
}
