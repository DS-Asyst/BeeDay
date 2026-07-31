using System.Text.Json.Nodes;

namespace LevelUp.Infrastructure.Persistence.Json;

/// <summary>
/// Reads documents written before Inventory was renamed to Wallet (schema 6 and earlier):
/// renames the top-level "inventoryTags" array to "walletTags" and each transaction's
/// "inventoryTagId" foreign key to "walletTagId". No-op on documents that already use the
/// current shape.
/// </summary>
internal static class LegacyInventoryTagMigrator
{
    public static void Migrate(JsonNode? node)
    {
        if (node is not JsonObject root)
        {
            return;
        }

        if (root.TryGetPropertyValue("inventoryTags", out var inventoryTags))
        {
            root.Remove("inventoryTags");
            root["walletTags"] = inventoryTags;
        }

        if (root["transactions"] is JsonArray transactions)
        {
            foreach (var transactionNode in transactions)
            {
                if (transactionNode is JsonObject transaction
                    && transaction.TryGetPropertyValue("inventoryTagId", out var inventoryTagId))
                {
                    transaction.Remove("inventoryTagId");
                    transaction["walletTagId"] = inventoryTagId;
                }
            }
        }
    }
}
