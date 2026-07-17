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
            title: "Capítulo órfão",
            description: "Relacionamento inválido.",
            order: 1
        );

        GameData data = new()
        {
            Milestones = [milestone]
        };

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => new GameDataValidator().Validate(data)
        );

        Assert.Contains("projeto válido", exception.Message);
    }

    [Fact]
    public void Validator_ShouldRejectDuplicateProjectIds()
    {
        Project first = new() { Id = 1 };
        first.Configure("Primeiro", "Descrição");

        Project second = new() { Id = 1 };
        second.Configure("Segundo", "Descrição");

        GameData data = new()
        {
            Projects = [first, second]
        };

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => new GameDataValidator().Validate(data)
        );

        Assert.Contains("IDs duplicados", exception.Message);
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
