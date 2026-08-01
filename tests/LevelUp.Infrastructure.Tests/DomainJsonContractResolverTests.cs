using System.Text.Json;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.Persistence.Json;
using Microsoft.Extensions.Options;
using Xunit;

namespace LevelUp.Infrastructure.Tests;

/// <summary>
/// Domain no longer carries any System.Text.Json attribute (Sprint 12.8): these tests prove the
/// Infrastructure-only <see cref="DomainJsonContractResolver"/> reproduces, byte-for-byte in
/// shape, the exact contract the removed [JsonInclude]/[JsonPropertyName]/[JsonIgnore] attributes
/// used to define, purely by reflecting over each Domain type from Infrastructure.
/// </summary>
public sealed class DomainJsonContractResolverTests
{
    private readonly JsonSerializerOptions options = new JsonSerializerOptionsFactory(
        Options.Create(new JsonStorageOptions
        {
            Directory = "Data",
            FileName = "LevelUpBD.json",
            BackupDirectory = "Backups"
        })).Create();

    [Fact]
    public void Serialize_OmitsComputedProperties()
    {
        var data = new LevelUpData();
        var user = User.Create("Ada Lovelace", "ada@levelup.invalid");
        data.AddUser(user);
        data.CompleteUserProfile(user.Id, "adahero");
        var project = Project.Create("Launch", null);
        data.AddProject(project);
        project.AddTodo(Todo.Create(project.Id, "Write specs", null, null));

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(data, options));
        var root = document.RootElement;
        var userElement = root.GetProperty("users")[0];
        var projectElement = root.GetProperty("projects")[0];

        // LevelUpData: CurrentUser is computed from CurrentUserId (the legacy, unrelated "todos"
        // key legitimately still appears here — it belongs to the private LegacyTodos property,
        // always [] once EnsureValidState has run, not to the computed Todos property).
        Assert.False(root.TryGetProperty("currentUser", out _));

        // User: HasProfile / Profile are computed from Nickname, Name, Avatar, etc.
        Assert.False(userElement.TryGetProperty("hasProfile", out _));
        Assert.False(userElement.TryGetProperty("profile", out _));

        // Project: every computed member, including the Completed override with a no-op setter —
        // Habit/RecurringTask/Todo keep their own real "completed" field, only Project's is derived.
        foreach (var computed in new[] { "name", "totalTodos", "pendingTodos", "completedTodos", "progressPercentage", "progress", "lastUpdatedAtUtc", "nextTodo", "status", "completed" })
        {
            Assert.False(projectElement.TryGetProperty(computed, out _), $"'{computed}' should have been omitted from the Project contract.");
        }

        // Sanity: the real, persisted fields are still there.
        Assert.Equal("adahero", userElement.GetProperty("nickname").GetString());
        Assert.Equal("Write specs", projectElement.GetProperty("todos")[0].GetProperty("title").GetString());
    }

    [Fact]
    public void Serialize_KeepsHistoricalTransactionsKey_ForUserExperienceEntries()
    {
        var experience = UserExperience.Create();
        experience.Add(
            ExperienceReward.Create(100),
            ExperienceSource.Create(ExperienceSourceType.Manual));

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(experience, options));

        Assert.True(document.RootElement.TryGetProperty("Transactions", out var entries));
        Assert.Single(entries.EnumerateArray());
        Assert.False(document.RootElement.TryGetProperty("entries", out _));
    }

    [Fact]
    public void RoundTrip_PopulatesPrivateSetters_ThroughNestedCollections()
    {
        var data = new LevelUpData();
        var user = User.Create("Grace Hopper", "grace@levelup.invalid");
        data.AddUser(user);
        var project = Project.Create("Compiler", "Ship it");
        data.AddProject(project);
        project.AddTodo(Todo.Create(project.Id, "Write COBOL", "Priority task", new DateOnly(2026, 1, 1)));

        var json = JsonSerializer.Serialize(data, options);
        var roundTripped = JsonSerializer.Deserialize<LevelUpData>(json, options)!;

        var loadedProject = Assert.Single(roundTripped.Projects);
        Assert.Equal("Compiler", loadedProject.Title);
        Assert.Equal("Ship it", loadedProject.Description);
        var loadedTodo = Assert.Single(loadedProject.Todos);
        Assert.Equal("Write COBOL", loadedTodo.Title);
        Assert.Equal(new DateOnly(2026, 1, 1), loadedTodo.DueDate);
        Assert.Equal(loadedProject.Id, loadedTodo.ProjectId);
    }
}
