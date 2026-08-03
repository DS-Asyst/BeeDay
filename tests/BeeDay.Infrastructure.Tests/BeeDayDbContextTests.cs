using BeeDay.Domain.Entities;
using BeeDay.Domain.Experience;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.DependencyInjection;
using BeeDay.Infrastructure.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

public sealed class BeeDayDbContextTests
{
    private const string TestConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=BeeDayDbContextTests;Trusted_Connection=True;TrustServerCertificate=True;";

    [Fact]
    public void Model_BuildsWithoutThrowing()
    {
        using var context = CreateContext();

        Assert.NotNull(context.Model);
    }

    // Habit/RecurringTask/Project/Todo are included here now that Sprint 14.3 scopes
    // UseTpcMappingStrategy() to Activity — each gets its own fully-columned table, matching
    // 01-relational-model.md, instead of EF Core's table-per-hierarchy default.
    [Theory]
    [InlineData(typeof(User), "Users")]
    [InlineData(typeof(UserToken), "UserTokens")]
    [InlineData(typeof(Habit), "Habits")]
    [InlineData(typeof(RecurringTask), "RecurringTasks")]
    [InlineData(typeof(Project), "Projects")]
    [InlineData(typeof(Todo), "Todos")]
    [InlineData(typeof(Wallet), "Wallets")]
    [InlineData(typeof(WalletTag), "WalletTags")]
    [InlineData(typeof(Transaction), "Transactions")]
    [InlineData(typeof(ExperienceEntry), "ExperienceEntries")]
    public void EachEntity_MapsToItsOwnTableWithTheExpectedName(Type clrType, string expectedTableName)
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(clrType);

        Assert.NotNull(entityType);
        Assert.Equal(expectedTableName, entityType!.GetTableName());
    }

    [Fact]
    public void ActivityDerivedEntities_MapToFourDistinctTables()
    {
        using var context = CreateContext();

        var tableNames = new[] { typeof(Habit), typeof(RecurringTask), typeof(Project), typeof(Todo) }
            .Select(clrType => context.Model.FindEntityType(clrType)!.GetTableName())
            .ToArray();

        Assert.Equal(4, tableNames.Distinct().Count());
    }

    [Fact]
    public void UserExperience_MapsToOwnTableSharingUsersPrimaryKey()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(UserExperience));

        Assert.NotNull(entityType);
        Assert.Equal("UserExperience", entityType!.GetTableName());
        Assert.Equal(["UserId"], entityType.FindPrimaryKey()!.Properties.Select(p => p.Name));
    }

    [Fact]
    public void ExperienceSource_MapsAsInlineColumnsOnExperienceEntries()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(ExperienceEntry))!;
        var sourceComplexType = entityType.FindComplexProperty("Source")!.ComplexType;

        Assert.Equal("ExperienceEntries", entityType.GetTableName());
        Assert.Equal("SourceType", sourceComplexType.FindProperty("Type")!.GetColumnName());
        Assert.Equal("SourceId", sourceComplexType.FindProperty("ReferenceId")!.GetColumnName());
        Assert.Equal("SourceDescription", sourceComplexType.FindProperty("Description")!.GetColumnName());
    }

    [Theory]
    [InlineData(typeof(User))]
    [InlineData(typeof(UserToken))]
    [InlineData(typeof(Habit))]
    [InlineData(typeof(RecurringTask))]
    [InlineData(typeof(Project))]
    [InlineData(typeof(Todo))]
    [InlineData(typeof(Wallet))]
    [InlineData(typeof(WalletTag))]
    [InlineData(typeof(Transaction))]
    [InlineData(typeof(UserExperience))]
    public void MutableEntities_HaveARowVersionConcurrencyToken(Type clrType)
    {
        using var context = CreateContext();

        var rowVersion = context.Model.FindEntityType(clrType)!.FindProperty("RowVersion");

        Assert.NotNull(rowVersion);
        Assert.True(rowVersion!.IsConcurrencyToken);
    }

    [Theory]
    [InlineData(typeof(Todo), "UserId", DeleteBehavior.NoAction)]
    [InlineData(typeof(Todo), "ProjectId", DeleteBehavior.Cascade)]
    [InlineData(typeof(Wallet), "UserId", DeleteBehavior.NoAction)]
    [InlineData(typeof(Habit), "UserId", DeleteBehavior.Cascade)]
    [InlineData(typeof(WalletTag), "UserId", DeleteBehavior.Cascade)]
    [InlineData(typeof(Transaction), "WalletId", DeleteBehavior.Cascade)]
    [InlineData(typeof(Transaction), "WalletTagId", DeleteBehavior.SetNull)]
    public void ForeignKeys_HaveTheApprovedDeleteBehavior(Type dependentType, string foreignKeyProperty, DeleteBehavior expected)
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(dependentType)!;
        var foreignKey = entityType.GetForeignKeys()
            .Single(fk => fk.Properties.Any(p => p.Name == foreignKeyProperty));

        Assert.Equal(expected, foreignKey.DeleteBehavior);
    }

    // No test for UX_ExperienceEntries_Dedup here: confirmed during this Sprint that EF Core cannot
    // express an index spanning both ExperienceEntry's own properties and its Source complex type's
    // properties through any Fluent API or metadata surface — the index is added via raw SQL directly
    // in the InitialCreate migration instead (see ExperienceEntryConfiguration.cs for the full
    // rationale), so it is verified by inspecting the generated migration/script, not the EF model.

    [Fact]
    public void Users_HaveFilteredUniqueIndexOnNickname()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(User))!;
        var index = entityType.GetIndexes().Single(i => i.GetDatabaseName() == "UX_Users_Nickname");

        Assert.True(index.IsUnique);
        Assert.Equal("[Nickname] <> N''", index.GetFilter());
    }

    [Fact]
    public void ExperienceEntry_HasNoRowVersion()
    {
        using var context = CreateContext();

        var rowVersion = context.Model.FindEntityType(typeof(ExperienceEntry))!.FindProperty("RowVersion");

        Assert.Null(rowVersion);
    }

    [Fact]
    public void SqlServerOptions_BindsConnectionStringFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{SqlServerOptions.SectionName}:ConnectionString"] = TestConnectionString
            })
            .Build();

        var options = new SqlServerOptions();
        configuration.GetSection(SqlServerOptions.SectionName).Bind(options);

        Assert.Equal(TestConnectionString, options.ConnectionString);
    }

    [Fact]
    public void AddBeeDayInfrastructure_ResolvesDbContextFactoryWithoutThrowing()
    {
        // BeeDayDbContext itself is deliberately not resolvable directly from the container — only
        // IDbContextFactory<BeeDayDbContext> is registered (AddDbContextFactory, not AddDbContext),
        // because BeeDay.Web is Blazor Server and a scoped DbContext would live for the whole
        // long-lived circuit. Every consumer creates and disposes its own short-lived context.
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{SqlServerOptions.SectionName}:ConnectionString"] = TestConnectionString
            })
            .Build();

        services.AddBeeDayInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IDbContextFactory<BeeDayDbContext>>();
        using var context = factory.CreateDbContext();

        Assert.NotNull(context.Model);
    }

    private static BeeDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BeeDayDbContext>()
            .UseSqlServer(TestConnectionString)
            .Options;

        return new BeeDayDbContext(options);
    }
}
