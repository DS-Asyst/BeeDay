using LevelUp.Domain;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Services.Persistence;
using Xunit;

namespace LevelUp.Tests;

public sealed class GameDataValidatorTests
{
    [Fact]
    public void Validator_ShouldRejectMilestoneWithoutExistingProject()
    {
        Milestone milestone = new() { Id = 1 };
        milestone.Configure(
            projectId: 99,
            title: "Orphan milestone",
            description: "Invalid relationship.",
            order: 1
        );

        GameData data = new()
        {
            Milestones = [milestone]
        };

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => new GameDataValidator().Validate(data)
        );

        Assert.Contains("valid project", exception.Message);
    }

    [Fact]
    public void Validator_ShouldRejectDuplicateProjectIds()
    {
        Project first = new() { Id = 1 };
        first.Configure("First", "Description");

        Project second = new() { Id = 1 };
        second.Configure("Second", "Description");

        GameData data = new()
        {
            Projects = [first, second]
        };

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => new GameDataValidator().Validate(data)
        );

        Assert.Contains("Duplicate IDs", exception.Message);
    }
    [Fact]
    public void Migrator_ShouldUpgradeSchemaThreeToCurrentVersion()
    {
        GameData data = new()
        {
            SchemaVersion = 3
        };

        new LevelUp.Services.Persistence.Migrations.GameDataMigrator()
            .Migrate(data);

        Assert.Equal(
            GameData.CurrentSchemaVersion,
            data.SchemaVersion
        );
    }

}
