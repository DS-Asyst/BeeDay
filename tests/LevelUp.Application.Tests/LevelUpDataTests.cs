using System.Text.Json;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using Xunit;

namespace LevelUp.Application.Tests;

public sealed class LevelUpDataTests
{
    [Fact]
    public void EnsureValidState_RejectsDuplicateIds()
    {
        var id = Guid.NewGuid();
        var json = $$"""
        {
          "schemaVersion": 1,
          "habits": [
            { "id": "{{id}}", "title": "One" },
            { "id": "{{id}}", "title": "Two" }
          ]
        }
        """;
        var data = JsonSerializer.Deserialize<LevelUpData>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        Assert.Throws<InvalidDomainStateException>(data.EnsureValidState);
    }
}
