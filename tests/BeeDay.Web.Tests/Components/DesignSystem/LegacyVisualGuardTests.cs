namespace BeeDay.Web.Tests.Components.DesignSystem;

public sealed class LegacyVisualGuardTests
{
    [Fact]
    public void RuntimeDoesNotLoadRetiredPixelOrNesStylesheets()
    {
        var root = FindRepositoryRoot();
        var app = File.ReadAllText(Path.Combine(root, "src", "BeeDay.Web", "Components", "App.razor"));
        Assert.DoesNotContain("pixel-nes.css", app, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pixel-ui.css", app, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(Path.Combine(root, "src", "BeeDay.Web", "wwwroot", "css", "pixel-nes.css")));
        Assert.False(File.Exists(Path.Combine(root, "src", "BeeDay.Web", "wwwroot", "css", "vendor", "nes-core.beeday-excerpt.css")));
    }

    [Fact]
    public void RazorConsumersDoNotReintroduceComicOrPixelClasses()
    {
        var web = Path.Combine(FindRepositoryRoot(), "src", "BeeDay.Web");
        var matches = Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories)
            .SelectMany(File.ReadAllLines)
            .Where(line => line.Contains("beeday-button--comic", StringComparison.OrdinalIgnoreCase)
                || line.Contains("beeday-pixel-panel", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(matches);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "BeeDay.slnx")))
        {
            current = current.Parent;
        }
        return current?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
