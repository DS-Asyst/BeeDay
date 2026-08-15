using System.Text.RegularExpressions;

namespace BeeDay.Web.Tests.Components.Visual;

public sealed class EntryFlowVisualConsistencyTests
{
    [Fact]
    public void EntryPagesUseSingleSharedBrandContract()
    {
        var root = FindRepositoryRoot();
        var pagePaths = Directory.GetFiles(
            Path.Combine(root, "src", "BeeDay.Web", "Components", "Features"),
            "*.razor",
            SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}Authentication{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || path.Contains($"{Path.DirectorySeparatorChar}Identity{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || path.Contains($"{Path.DirectorySeparatorChar}ProfileCreation{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || path.Contains($"{Path.DirectorySeparatorChar}Onboarding{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        var source = string.Join('\n', pagePaths.Select(File.ReadAllText));

        Assert.Contains("<BeeDayBrand", source);
        Assert.DoesNotContain("identity" + "-brand", source);
        Assert.DoesNotContain("<span>LEVEL</span>" + "<span>UP</span>", source);
    }

    [Fact]
    public void ReplacedEntryFlowStylesAndTutorialArtifactsAreAbsent()
    {
        var root = FindRepositoryRoot();
        var sourceRoots = new[]
        {
            Path.Combine(root, "src")
        };
        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".razor", ".cs", ".css" };
        var source = string.Join('\n', sourceRoots
            .SelectMany(path => Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            .Where(path => extensions.Contains(Path.GetExtension(path)))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(File.ReadAllText));

        Assert.DoesNotContain("tutorial" + "-button", source);
        Assert.DoesNotContain("character" + "-primary-button", source);
        Assert.DoesNotContain("character" + "-back-button", source);
        Assert.DoesNotContain("tutorial" + "-progress-bar", source);
        Assert.False(Regex.IsMatch(source, @"\bProgressPercent\b", RegexOptions.CultureInvariant));
    }

    [Fact]
    public void OnboardingLayoutHasNoBackgroundImage()
    {
        var root = FindRepositoryRoot();

        var layoutCss = File.ReadAllText(Path.Combine(
            root, "src", "BeeDay.Web", "Components", "Layout", "OnboardingLayout.razor.css"));
        Assert.DoesNotContain("background-image", layoutCss, StringComparison.Ordinal);
        Assert.DoesNotContain("url(", layoutCss, StringComparison.Ordinal);

        var componentsDir = Path.Combine(
            root, "src", "BeeDay.Web", "Components", "Features", "Authentication", "Components");
        Assert.False(
            File.Exists(Path.Combine(componentsDir, "LoginBackground.razor")),
            "LoginBackground was removed in Sprint 20.8 (EPIC 20) — Login/Identity/Onboarding no longer render an image background.");

        Assert.False(Directory.Exists(Path.Combine(root, "src", "BeeDay.Web", "wwwroot", "images", "authentication")));
        Assert.False(Directory.Exists(Path.Combine(root, "src", "BeeDay.Web", "wwwroot", "images", "onboarding")));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the BeeDay repository root.");
    }
}
