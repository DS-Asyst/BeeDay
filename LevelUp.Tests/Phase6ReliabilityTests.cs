using System.Text.Json;
using LevelUp.Domain;
using LevelUp.Domain.Projects;
using LevelUp.Services.Persistence;
using LevelUp.Services.Wallet;
using Xunit;

namespace LevelUp.Tests;

public sealed class Phase6ReliabilityTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"LevelUpPhase6_{Guid.NewGuid():N}");

    [Fact]
    public void Load_ShouldMigrateLegacySaveAndCreateFinalBoss()
    {
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "save.json");
        Project project = new() { Id = 1 };
        project.Configure("Projeto legado", "Descrição");
        string json = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1,
            Character = new { },
            Habits = Array.Empty<object>(),
            Projects = new[] { project },
            Quests = Array.Empty<object>(),
            Milestones = Array.Empty<object>(),
            Bosses = Array.Empty<object>(),
            Books = Array.Empty<object>(),
            WalletTransactions = Array.Empty<object>(),
            Achievements = Array.Empty<object>()
        });
        File.WriteAllText(path, json);

        GameData? loaded = new SaveService(path).Load();

        Assert.NotNull(loaded);
        Assert.Equal(GameData.CurrentSchemaVersion, loaded.SchemaVersion);
        Assert.Single(loaded.Bosses);
        Assert.Equal(project.Id, loaded.Bosses.Single().ProjectId);
    }

    [Fact]
    public void Save_ShouldKeepPreviousValidSnapshot()
    {
        string path = Path.Combine(directory, "save.json");
        SaveService service = new(path);
        service.Save(new GameData());
        service.Save(new GameData());

        Assert.True(File.Exists(path));
        Assert.True(File.Exists(path + ".previous"));
    }

    [Fact]
    public void ReverseTransaction_ShouldPreserveHistoryAndRestoreBalance()
    {
        WalletService service = new();
        var deposit = service.AddDeposit(500m, "Reserva", new DateTime(2026, 7, 1));

        var reversal = service.ReverseTransaction(
            deposit,
            "Lançamento duplicado",
            new DateTime(2026, 7, 2)
        );

        Assert.Equal(0m, service.Balance);
        Assert.True(deposit.IsReversed);
        Assert.True(reversal.IsReversal);
        Assert.Equal(deposit.Id, reversal.ReversalOfTransactionId);
        Assert.Equal(2, service.GetAll().Count);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}
