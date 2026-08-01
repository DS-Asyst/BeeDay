using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Experience;

namespace LevelUp.Infrastructure.Persistence.Json;

/// <summary>
/// Lets System.Text.Json read and write Domain entities' private-setter properties and preserve
/// legacy JSON property names, without a single [JsonInclude]/[JsonPropertyName]/[JsonIgnore]
/// attribute inside LevelUp.Domain. This recreates, entirely from Infrastructure, exactly what
/// those attributes used to do:
///
/// - a property backed by a compiler-generated auto-property field is real, persisted state: its
///   setter (of any visibility) is wired up so deserialization can populate it — this is the old
///   [JsonInclude] behavior, and it needs no per-property list since every such property shares
///   the same shape (public getter, non-public auto-property setter);
/// - a property with no backing field — a computed, expression-bodied property, including a
///   no-op override such as <see cref="Project.Completed"/> — is derived, never persisted state:
///   it is removed from the contract, matching the old [JsonIgnore] behavior. This also needs no
///   per-property list: the backing-field probe is what [JsonIgnore] was manually encoding;
/// - three historical renames (kept only for existing JSON file compatibility) are restored
///   explicitly below, matching the old [JsonPropertyName] usages;
/// - two private properties used only to read pre-existing legacy documents
///   (<see cref="LevelUpData"/>'s "profile" and "todos") are added back explicitly, since the
///   default resolver never reflects non-public properties at all.
/// </summary>
internal static class DomainJsonContractResolver
{
    private static readonly Dictionary<(Type Type, string ClrName), string> RenamedProperties = new()
    {
        [(typeof(UserExperience), nameof(UserExperience.Entries))] = "Transactions",
    };

    private static readonly (Type Type, string ClrPropertyName, string JsonName)[] LegacyPrivateProperties =
    [
        (typeof(LevelUpData), "LegacyProfile", "profile"),
        (typeof(LevelUpData), "LegacyTodos", "todos"),
    ];

    public static IJsonTypeInfoResolver Create() =>
        new DefaultJsonTypeInfoResolver().WithAddedModifier(Modify);

    private static readonly Assembly DomainAssembly = typeof(LevelUpData).Assembly;

    private static void Modify(JsonTypeInfo typeInfo)
    {
        // Scoped to LevelUp.Domain only: other types serialized through the same options (e.g.
        // JsonEventJournal's anonymous envelope) already serialize correctly by default and must
        // not be touched — the backing-field probe below is specific to how the Domain's own
        // auto-properties are shaped, not a general-purpose contract rule.
        if (typeInfo.Kind != JsonTypeInfoKind.Object || typeInfo.Type.Assembly != DomainAssembly)
        {
            return;
        }

        RemoveComputedProperties(typeInfo);
        WireNonPublicSetters(typeInfo);
        ApplyRenames(typeInfo);
        AddLegacyPrivateProperties(typeInfo);
    }

    /// <summary>
    /// A property with a public getter but no compiler-generated backing field has no real state
    /// to deserialize into — it is computed (an expression-bodied property, or a no-op setter
    /// override like <see cref="Project.Completed"/>) and must never appear in the persisted file.
    /// </summary>
    private static void RemoveComputedProperties(JsonTypeInfo typeInfo)
    {
        for (var index = typeInfo.Properties.Count - 1; index >= 0; index--)
        {
            if (typeInfo.Properties[index].AttributeProvider is PropertyInfo property && !HasBackingField(property))
            {
                typeInfo.Properties.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// The default resolver only wires up accessible setters. Every property kept by
    /// <see cref="RemoveComputedProperties"/> has a real backing field, so its setter — private,
    /// protected, or public — is the one the constructor/mutator methods already trust; this just
    /// lets deserialization use it too.
    /// </summary>
    private static void WireNonPublicSetters(JsonTypeInfo typeInfo)
    {
        foreach (var jsonProperty in typeInfo.Properties)
        {
            if (jsonProperty.Set is not null || jsonProperty.AttributeProvider is not PropertyInfo property || property.SetMethod is null)
            {
                continue;
            }

            jsonProperty.Set = (obj, value) => property.SetValue(obj, value);
        }
    }

    private static void ApplyRenames(JsonTypeInfo typeInfo)
    {
        foreach (var jsonProperty in typeInfo.Properties)
        {
            if (jsonProperty.AttributeProvider is PropertyInfo property
                && RenamedProperties.TryGetValue((typeInfo.Type, property.Name), out var jsonName))
            {
                jsonProperty.Name = jsonName;
            }
        }
    }

    private static void AddLegacyPrivateProperties(JsonTypeInfo typeInfo)
    {
        foreach (var (type, clrName, jsonName) in LegacyPrivateProperties)
        {
            if (typeInfo.Type != type)
            {
                continue;
            }

            var property = type.GetProperty(clrName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Expected a private property '{clrName}' on '{type.Name}'.");

            var jsonProperty = typeInfo.CreateJsonPropertyInfo(property.PropertyType, jsonName);
            jsonProperty.Get = property.GetValue;
            jsonProperty.Set = (obj, value) => property.SetValue(obj, value);
            typeInfo.Properties.Add(jsonProperty);
        }
    }

    private static bool HasBackingField(PropertyInfo property) =>
        property.DeclaringType?.GetField(
            $"<{property.Name}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) is not null;
}
