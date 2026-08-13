namespace BeeDay.Web.Tests.Components.Home;

public sealed class AuthenticatedHomeContractTests
{
    private static readonly string Source = File.ReadAllText(Path.Combine(
        ResolveRepoRoot(), "src", "BeeDay.Web", "Components", "Features", "Dashboard", "Pages", "DashboardHome.razor"));

    [Fact]
    public void UsesCanonicalRouteSharedStateAndOfficialPrimitives()
    {
        Assert.Contains("@page \"/home\"", Source, StringComparison.Ordinal);
        Assert.Contains("Dashboard.State.DashboardState State", Source, StringComparison.Ordinal);
        Assert.Contains("<BeeDayCard", Source, StringComparison.Ordinal);
        Assert.Contains("<BeeDayButton", Source, StringComparison.Ordinal);
        Assert.Contains("<BeeDayIcon", Source, StringComparison.Ordinal);
        Assert.Contains("<BeeDayProgressBar", Source, StringComparison.Ordinal);
        Assert.Contains("<ExperienceBar", Source, StringComparison.Ordinal);
    }

    [Fact]
    public void PreservesDailyAsOperationalDestinationAndRejectsFictionalGamification()
    {
        Assert.Contains("/daily", Source, StringComparison.Ordinal);
        Assert.DoesNotContain("Streak", Source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Achievement", Source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Quest", Source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("League", Source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("+20 XP", Source, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
