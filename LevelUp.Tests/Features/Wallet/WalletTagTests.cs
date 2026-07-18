using LevelUp.Domain.Wallet;
using LevelUp.Services.Wallet;
using Xunit;

namespace LevelUp.Tests;

public sealed class WalletTagTests
{
    [Fact]
    public void EntryAndExit_ShouldUseUserDefinedTags()
    {
        WalletService service = new();
        WalletTag income = service.CreateTag("Salário");
        WalletTag food = service.CreateTag("Alimentação");

        WalletTransaction entry = service.AddEntry(
            1000m,
            "Salário mensal",
            income,
            new DateTime(2026, 7, 1)
        );
        WalletTransaction exit = service.AddExit(
            125m,
            "Almoço",
            food,
            new DateTime(2026, 7, 2)
        );

        Assert.Equal(income.Id, entry.TagId);
        Assert.Equal(food.Id, exit.TagId);
        Assert.Equal(875m, service.Balance);
    }

    [Fact]
    public void TagInUse_ShouldNotBeDeleted()
    {
        WalletService service = new();
        WalletTag tag = service.CreateTag("Educação");
        service.AddExit(
            50m,
            "Book",
            tag,
            new DateTime(2026, 7, 1)
        );

        Assert.Throws<InvalidOperationException>(
            () => service.DeleteTag(tag.Id)
        );
    }

    [Fact]
    public void DuplicateTagName_ShouldBeRejectedIgnoringCase()
    {
        WalletService service = new();
        service.CreateTag("Investimento");

        Assert.Throws<InvalidOperationException>(
            () => service.CreateTag("investimento")
        );
    }
}
