using System.Text.Json;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class LevelUpDataTests
{
    [Fact]
    public void NewData_StartsWithoutUser()
    {
        var data = new LevelUpData();

        data.EnsureValidState();

        Assert.Empty(data.Users);
        Assert.Null(data.CurrentUserId);
        Assert.Null(data.CurrentUser);
    }

    [Fact]
    public void ActivityCreation_RequiresCurrentUser()
    {
        var data = new LevelUpData();

        Assert.Throws<InvalidDomainStateException>(() =>
            data.AddHabit(Habit.Create("Study", null, LevelUp.Domain.Enums.HabitDirection.Positive, LevelUp.Domain.Enums.HabitDifficulty.Easy, LevelUp.Domain.Enums.HabitResetCounter.Daily)));
    }

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
