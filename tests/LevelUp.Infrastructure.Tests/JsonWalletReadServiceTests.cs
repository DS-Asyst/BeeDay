using LevelUp.Application.Features.Wallets.Contracts;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.Persistence.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace LevelUp.Infrastructure.Tests;

/// <summary>
/// Validates the real JSON adapter for <see cref="IWalletReadService"/> — a real temp file on disk
/// through <see cref="JsonLevelUpDocumentStore"/>, not a fake, per
/// docs/testing/01-testing-strategy.md.
/// </summary>
public sealed class JsonWalletReadServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"levelup-wallet-read-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task GetSummaryAsync_ReturnsNull_WhenUserHasNoWallet()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;

        var summary = await fixture.ReadService.GetSummaryAsync(Guid.NewGuid(), cancellationToken);

        Assert.Null(summary);
    }

    [Fact]
    public async Task GetSummaryAsync_ComputesBalanceIncomeExpensesAndCount()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        var (user, wallet) = await fixture.SeedUserWithWalletAsync(cancellationToken);
        await fixture.Store.MutateAsync(data =>
        {
            data.AddTransaction(Transaction.Create(wallet.Id, "Salary", 1500m, TransactionType.Income, new DateOnly(2026, 7, 25)));
            data.AddTransaction(Transaction.Create(wallet.Id, "Internet", 80m, TransactionType.Expense, new DateOnly(2026, 7, 25)));
        }, cancellationToken);

        var summary = await fixture.ReadService.GetSummaryAsync(user.Id, cancellationToken);

        Assert.NotNull(summary);
        Assert.Equal(1420m, summary.Balance);
        Assert.Equal(1500m, summary.TotalIncome);
        Assert.Equal(80m, summary.TotalExpenses);
        Assert.Equal(2, summary.TransactionCount);
    }

    [Fact]
    public async Task ListTagsAsync_ReturnsOnlyCurrentUsersTags_WithTransactionCounts()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        var (user, wallet) = await fixture.SeedUserWithWalletAsync(cancellationToken);
        var other = User.Create("Other", $"{Guid.NewGuid():N}@levelup.test");
        WalletTag tag = null!;
        await fixture.Store.MutateAsync(data =>
        {
            data.AddUser(other);
            tag = WalletTag.Create(user.Id, "Food");
            data.AddWalletTag(tag);
            data.AddWalletTag(WalletTag.Create(other.Id, "Food"));
            data.AddTransaction(Transaction.Create(wallet.Id, "Lunch", 20m, TransactionType.Expense, new DateOnly(2026, 7, 25), tag.Id));
        }, cancellationToken);

        var tags = await fixture.ReadService.ListTagsAsync(user.Id, cancellationToken);

        var found = Assert.Single(tags);
        Assert.Equal(tag.Id, found.Id);
        Assert.Equal(1, found.TransactionCount);
    }

    [Fact]
    public async Task GetTransactionAsync_ReturnsNull_WhenTransactionBelongsToAnotherWallet()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, wallet) = await fixture.SeedUserWithWalletAsync(cancellationToken);
        var (otherUser, otherWallet) = await fixture.SeedUserWithWalletAsync(cancellationToken);
        Transaction transaction = null!;
        await fixture.Store.MutateAsync(data =>
        {
            transaction = Transaction.Create(wallet.Id, "Private", 10m, TransactionType.Expense, new DateOnly(2026, 7, 25));
            data.AddTransaction(transaction);
        }, cancellationToken);

        var result = await fixture.ReadService.GetTransactionAsync(otherUser.Id, transaction.Id, cancellationToken);

        Assert.Null(result);
        _ = otherWallet;
    }

    [Fact]
    public async Task ListTransactionsAsync_FiltersSortsAndPaginates()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        var (user, wallet) = await fixture.SeedUserWithWalletAsync(cancellationToken);
        await fixture.Store.MutateAsync(data =>
        {
            data.AddTransaction(Transaction.Create(wallet.Id, "Salary", 1000m, TransactionType.Income, new DateOnly(2026, 7, 1), notes: "Monthly"));
            data.AddTransaction(Transaction.Create(wallet.Id, "Groceries", 200m, TransactionType.Expense, new DateOnly(2026, 7, 2), notes: "Food"));
            data.AddTransaction(Transaction.Create(wallet.Id, "Coffee", 10m, TransactionType.Expense, new DateOnly(2026, 7, 3), notes: "Food"));
        }, cancellationToken);

        var result = await fixture.ReadService.ListTransactionsAsync(
            user.Id,
            new TransactionQueryFilter(
                Search: "Food",
                Type: TransactionType.Expense,
                SortBy: TransactionSortField.Amount,
                SortDirection: SortDirection.Descending,
                Page: 1,
                PageSize: 1),
            cancellationToken);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal("Groceries", Assert.Single(result.Items).Description);
    }

    [Fact]
    public async Task ListTransactionsAsync_IsolatesTransactionsBetweenUsers()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        var (userA, walletA) = await fixture.SeedUserWithWalletAsync(cancellationToken);
        var (userB, walletB) = await fixture.SeedUserWithWalletAsync(cancellationToken);
        await fixture.Store.MutateAsync(data =>
        {
            data.AddTransaction(Transaction.Create(walletA.Id, "A's transaction", 50m, TransactionType.Expense, new DateOnly(2026, 7, 1)));
            data.AddTransaction(Transaction.Create(walletB.Id, "B's transaction", 75m, TransactionType.Expense, new DateOnly(2026, 7, 1)));
        }, cancellationToken);

        var resultA = await fixture.ReadService.ListTransactionsAsync(userA.Id, new TransactionQueryFilter(), cancellationToken);
        var resultB = await fixture.ReadService.ListTransactionsAsync(userB.Id, new TransactionQueryFilter(), cancellationToken);

        Assert.Equal("A's transaction", Assert.Single(resultA.Items).Description);
        Assert.Equal("B's transaction", Assert.Single(resultB.Items).Description);
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
        var readService = new JsonWalletReadService(store);

        return new Fixture(store, readService);
    }

    private sealed record Fixture(JsonLevelUpDocumentStore Store, JsonWalletReadService ReadService)
    {
        public async Task<(User User, Wallet Wallet)> SeedUserWithWalletAsync(CancellationToken cancellationToken)
        {
            User user = null!;
            Wallet wallet = null!;
            await Store.MutateAsync(data =>
            {
                user = User.Create("Wallet Read Test User", $"{Guid.NewGuid():N}@levelup.test");
                data.AddUser(user);
                wallet = Wallet.Create(user.Id);
                data.AddWallet(wallet);
            }, cancellationToken);
            return (user, wallet);
        }
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "LevelUp.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
