using System.Text.Json;
using System.Text.Json.Serialization;
using LevelUp.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace LevelUp.Infrastructure.Persistence.Json;

public sealed class JsonSerializerOptionsFactory(IOptions<JsonStorageOptions> options)
{
    public JsonSerializerOptions Create() => new(JsonSerializerDefaults.Web)
    {
        WriteIndented = options.Value.WriteIndented,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
