using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.Persistence.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace LevelUp.Infrastructure.Tests;

/// <summary>
/// Golden-master tests locking the current persistence contract (User profile + Wallet + Wallet
/// tags) from the Product Identity and Domain Simplification EPIC. Also verifies that documents
/// written before the Sprint 2 Character merge and the Sprint 3 Inventory-&gt;Wallet rename
/// (schemaVersion 5 and earlier) still migrate and load correctly.
/// </summary>
public sealed class SchemaCompatibilityCharacterizationTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"levelup-schema-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task SaveAndLoadAsync_PreservesUserProfileWalletAndWalletTagState()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;

        var user = User.Create("Ada Lovelace", "ada@levelup.invalid");
        var data = new LevelUpData();
        data.AddUser(user);
        data.CompleteUserProfile(user.Id, "adahero");
        user.AddExperience(
            ExperienceReward.Create(100),
            ExperienceSource.Create(ExperienceSourceType.Manual));
        user.AddExperience(
            ExperienceReward.Create(150),
            ExperienceSource.Create(ExperienceSourceType.Manual));

        var wallet = Wallet.Create(user.Id);
        data.AddWallet(wallet);

        var tag = WalletTag.Create(user.Id, "Groceries");
        data.AddWalletTag(tag);

        data.AddTransaction(Transaction.Create(
            wallet.Id, "Salary", 5000m, TransactionType.Income, new DateOnly(2026, 1, 5)));
        data.AddTransaction(Transaction.Create(
            wallet.Id, "Groceries run", 120.50m, TransactionType.Expense, new DateOnly(2026, 1, 6), tag.Id));

        await fixture.Repository.SaveAsync(data, cancellationToken);
        var loaded = await fixture.Repository.LoadAsync(cancellationToken);

        var loadedUser = Assert.Single(loaded.Users);
        Assert.Equal("Ada Lovelace", loadedUser.Name);
        Assert.Equal("ada@levelup.invalid", loadedUser.Email);
        Assert.True(loadedUser.HasProfile);
        Assert.Equal("adahero", loadedUser.Nickname);
        Assert.Equal(250, loadedUser.Experience.TotalExperience);
        Assert.Equal(2, loadedUser.Experience.CurrentLevel);
        Assert.Equal(2, loadedUser.Experience.Entries.Count);

        var loadedWallet = Assert.Single(loaded.Wallets);
        Assert.Equal(loadedUser.Id, loadedWallet.UserId);
        Assert.Equal(2, loaded.Transactions.Count);
        Assert.Equal(4879.50m, loadedWallet.CalculateBalance(loaded.Transactions));

        var loadedTag = Assert.Single(loaded.WalletTags);
        Assert.Equal("Groceries", loadedTag.Name);
        Assert.Contains(loaded.Transactions, transaction => transaction.WalletTagId == loadedTag.Id);
    }

    [Fact]
    public async Task LoadAsync_MigratesPreSprint2Schema_PreservingProfileExperienceAndWalletData()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        Directory.CreateDirectory(fixture.Paths.StorageDirectory);
        await File.WriteAllTextAsync(fixture.Paths.DataFile, LegacyCharacterAndInventorySchemaJson, cancellationToken);

        var loaded = await fixture.Repository.LoadAsync(cancellationToken);

        Assert.Equal(7, loaded.SchemaVersion);

        var user = Assert.Single(loaded.Users);
        Assert.Equal("Legacy User", user.Name);
        Assert.Equal("legacy.user@levelup.invalid", user.Email);
        Assert.True(user.IsEmailConfirmed);
        Assert.True(user.HasProfile);
        Assert.Equal("legacyhero", user.Nickname);
        Assert.Equal(250, user.Experience.TotalExperience);
        Assert.Equal(2, user.Experience.CurrentLevel);
        Assert.Equal(2, user.Experience.Entries.Count);
        // The old "characterId" foreign key on each experience entry pointed at the Character's
        // own id; the migration must re-point it at the owning User's id, not merely rename the key.
        Assert.All(user.Experience.Entries, entry => Assert.Equal(user.Id, entry.UserId));

        var wallet = Assert.Single(loaded.Wallets);
        Assert.Equal(user.Id, wallet.UserId);
        Assert.Equal(2, loaded.Transactions.Count);
        Assert.Equal(4879.50m, wallet.CalculateBalance(loaded.Transactions));

        // The old top-level "inventoryTags" array and each transaction's "inventoryTagId" foreign
        // key must migrate to "walletTags" / "walletTagId" with values preserved.
        var tag = Assert.Single(loaded.WalletTags);
        Assert.Equal("Groceries", tag.Name);
        Assert.Contains(loaded.Transactions, transaction => transaction.WalletTagId == tag.Id);
    }

    [Fact]
    public async Task LoadAsync_KeepsProfileAndWalletDataIsolatedBetweenUsers()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;

        var owner = User.Create("Owner", "owner@levelup.invalid");
        var stranger = User.Create("Stranger", "stranger@levelup.invalid");
        var data = new LevelUpData();
        data.AddUser(owner);
        data.AddUser(stranger);
        data.CompleteUserProfile(owner.Id, "ownerhero");
        data.CompleteUserProfile(stranger.Id, "strangerhero");

        var ownerWallet = Wallet.Create(owner.Id);
        var strangerWallet = Wallet.Create(stranger.Id);
        data.AddWallet(ownerWallet);
        data.AddWallet(strangerWallet);

        var ownerTag = WalletTag.Create(owner.Id, "Owner tag");
        data.AddWalletTag(ownerTag);

        await fixture.Repository.SaveAsync(data, cancellationToken);
        var loaded = await fixture.Repository.LoadAsync(cancellationToken);

        Assert.Equal("ownerhero", loaded.Users.Single(u => u.Id == owner.Id).Nickname);
        Assert.Equal("strangerhero", loaded.Users.Single(u => u.Id == stranger.Id).Nickname);
        Assert.DoesNotContain(loaded.WalletTags, tag => tag.UserId == stranger.Id);
        Assert.Single(loaded.WalletTags, tag => tag.UserId == owner.Id);
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private Fixture CreateFixture()
    {
        Directory.CreateDirectory(root);
        var options = Options.Create(new JsonStorageOptions
        {
            Directory = "Data",
            FileName = "LevelUpBD.json",
            BackupDirectory = "Backups",
            BackupRetention = 10,
            CreateBackupBeforeSave = true,
            RecoverFromBackup = true,
            WriteIndented = true
        });
        var environment = new TestHostEnvironment(root);
        var paths = new JsonStoragePaths(environment, options);
        var serializerFactory = new JsonSerializerOptionsFactory(options);
        var reader = new JsonFileReader(serializerFactory, NullLogger<JsonFileReader>.Instance);
        var writer = new JsonFileWriter(serializerFactory);
        var backups = new JsonBackupService(paths, options, reader, NullLogger<JsonBackupService>.Instance);
        var store = new JsonLevelUpDocumentStore(
            paths,
            reader,
            writer,
            backups,
            new JsonStorageGate(),
            new JsonStorageInitializer(paths),
            new JsonAtomicFileCommitter(),
            options,
            NullLogger<JsonLevelUpDocumentStore>.Instance);
        var repository = new JsonLevelUpRepository(store);

        return new Fixture(repository, paths);
    }

    private sealed record Fixture(JsonLevelUpRepository Repository, JsonStoragePaths Paths);

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "LevelUp.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    /// <summary>
    /// Snapshot matching the real on-disk shape from before Sprint 2/3 (schemaVersion 5:
    /// top-level "characters" array with a "class" and nested "experience" whose entries carry
    /// "characterId", top-level "inventoryTags", transactions carrying "inventoryTagId"). This is
    /// the reference fixture both the Character merge and the Inventory-&gt;Wallet rename
    /// migrations must keep reading correctly.
    /// </summary>
    private const string LegacyCharacterAndInventorySchemaJson = """
    {
      "schemaVersion": 5,
      "currentUserId": "11111111-1111-1111-1111-111111111111",
      "users": [
        {
          "id": "11111111-1111-1111-1111-111111111111",
          "name": "Legacy User",
          "email": "legacy.user@levelup.invalid",
          "passwordHash": "legacy-hash",
          "language": "English",
          "theme": "System",
          "createdAtUtc": "2026-01-01T00:00:00+00:00",
          "updatedAtUtc": "2026-01-01T00:00:00+00:00",
          "lastLoginAtUtc": null,
          "isActive": true,
          "hasCompletedOnboarding": true,
          "isEmailConfirmed": true,
          "emailConfirmedAtUtc": "2026-01-01T00:00:00+00:00"
        }
      ],
      "userTokens": [],
      "characters": [
        {
          "id": "22222222-2222-2222-2222-222222222222",
          "userId": "11111111-1111-1111-1111-111111111111",
          "nickname": "legacyhero",
          "class": "Warrior",
          "avatar": "",
          "experience": {
            "totalExperience": 250,
            "Transactions": [
              {
                "id": "33333333-3333-3333-3333-333333333333",
                "characterId": "22222222-2222-2222-2222-222222222222",
                "amount": 100,
                "source": { "type": "Manual", "referenceId": null, "description": "" },
                "rewardType": "Completion",
                "experienceBefore": 0,
                "experienceAfter": 100,
                "levelBefore": 1,
                "levelAfter": 2,
                "grantedAtUtc": "2026-01-02T00:00:00+00:00"
              },
              {
                "id": "44444444-4444-4444-4444-444444444444",
                "characterId": "22222222-2222-2222-2222-222222222222",
                "amount": 150,
                "source": { "type": "Manual", "referenceId": null, "description": "" },
                "rewardType": "Completion",
                "experienceBefore": 100,
                "experienceAfter": 250,
                "levelBefore": 2,
                "levelAfter": 2,
                "grantedAtUtc": "2026-01-03T00:00:00+00:00"
              }
            ]
          },
          "createdAtUtc": "2026-01-01T00:00:00+00:00",
          "updatedAtUtc": "2026-01-03T00:00:00+00:00"
        }
      ],
      "habits": [],
      "tasks": [],
      "projects": [],
      "wallets": [
        {
          "id": "55555555-5555-5555-5555-555555555555",
          "userId": "11111111-1111-1111-1111-111111111111",
          "createdAtUtc": "2026-01-01T00:00:00+00:00",
          "updatedAtUtc": "2026-01-06T00:00:00+00:00"
        }
      ],
      "transactions": [
        {
          "id": "66666666-6666-6666-6666-666666666666",
          "walletId": "55555555-5555-5555-5555-555555555555",
          "description": "Salary",
          "amount": 5000,
          "type": "Income",
          "transactionDate": "2026-01-05",
          "inventoryTagId": null,
          "notes": "",
          "createdAtUtc": "2026-01-05T00:00:00+00:00",
          "updatedAtUtc": "2026-01-05T00:00:00+00:00"
        },
        {
          "id": "77777777-7777-7777-7777-777777777777",
          "walletId": "55555555-5555-5555-5555-555555555555",
          "description": "Groceries run",
          "amount": 120.50,
          "type": "Expense",
          "transactionDate": "2026-01-06",
          "inventoryTagId": "88888888-8888-8888-8888-888888888888",
          "notes": "Weekly shop",
          "createdAtUtc": "2026-01-06T00:00:00+00:00",
          "updatedAtUtc": "2026-01-06T00:00:00+00:00"
        }
      ],
      "inventoryTags": [
        {
          "id": "88888888-8888-8888-8888-888888888888",
          "userId": "11111111-1111-1111-1111-111111111111",
          "name": "Groceries",
          "color": "#7A4FCB",
          "createdAtUtc": "2026-01-01T00:00:00+00:00",
          "updatedAtUtc": "2026-01-01T00:00:00+00:00"
        }
      ],
      "todos": []
    }
    """;
}
