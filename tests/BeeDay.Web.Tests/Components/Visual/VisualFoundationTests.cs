namespace BeeDay.Web.Tests.Components.Visual;

/// <summary>
/// Guards the global visual foundation established by Epic 21 Sprint 21.4. These are source-level
/// contracts because bUnit does not compute linked stylesheets or download web fonts.
/// </summary>
public sealed class VisualFoundationTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();

    [Fact]
    public void BrandFamilyUsesTheOfficialBeeDayPaletteWithoutAParallelNamespace()
    {
        var css = ReadWebFile("wwwroot", "css", "variables.css");

        Assert.Contains("--beeday-color-brand-primary: #3044d6;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-hover: #2739c4;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-active: #1f2faa;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-soft: #eef0ff;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-yellow: #ffd326;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-yellow-hover: #e8bd00;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-yellow-foreground: #2f2737;", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#1023c8", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#1e33ed", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#0c1b99", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--beeday-game-yellow", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--lingo-", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--epic21-", css, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublicHomeAcquisitionColorsAreContextualTokens()
    {
        var variables = ReadWebFile("wwwroot", "css", "variables.css");
        var home = ReadWebFile("Components", "Features", "Home", "Pages", "Home.razor.css");

        Assert.Contains("--beeday-color-public-home-cta: #14adff;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-public-home-cta-hover: #2cbaff;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-public-home-cta-active: #0798e2;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("var(--beeday-color-public-home-cta)", home, StringComparison.Ordinal);
        Assert.DoesNotContain("#14adff", home, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#2cbaff", home, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NunitoIsTheOnlyProductFontAndJerseyArtifactsAreRemoved()
    {
        var typography = ReadWebFile("wwwroot", "css", "typography.css");
        var app = ReadWebFile("Components", "App.razor");
        var sourceFiles = Directory.EnumerateFiles(
            Path.Combine(RepoRoot, "src", "BeeDay.Web"),
            "*",
            SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => Path.GetExtension(path) is ".css" or ".razor" or ".cs")
            .Select(File.ReadAllText);

        Assert.Contains("--beeday-font-family: var(--beeday-font-body);", typography, StringComparison.Ordinal);
        Assert.Contains("family=Nunito:wght@400;500;600;700;800;900", app, StringComparison.Ordinal);
        Assert.DoesNotContain("Jersey", app, StringComparison.OrdinalIgnoreCase);
        Assert.All(sourceFiles, content =>
        {
            Assert.DoesNotContain("Jersey 25", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("--beeday-font-ui", content, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void GlobalBackgroundIsASolidFoundationSurface()
    {
        var css = ReadWebFile("wwwroot", "app.css");

        Assert.Contains("body { background: var(--beeday-color-background); }", css, StringComparison.Ordinal);
        Assert.DoesNotContain("repeating-linear-gradient", css, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NeutralNavigationConsumesSharedMotionAndBrandFocusFoundations()
    {
        var navigationItem = ReadWebFile("Components", "Layout", "NavigationItem.razor.css");
        var mobileSidebar = ReadWebFile("Components", "Layout", "MobileSidebar.razor.css");

        Assert.Contains("var(--beeday-transition-normal)", navigationItem, StringComparison.Ordinal);
        Assert.Contains("var(--beeday-color-brand-primary)", navigationItem, StringComparison.Ordinal);
        Assert.Contains("var(--beeday-transition-emphasized)", mobileSidebar, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", mobileSidebar, StringComparison.Ordinal);
    }

    private static string ReadWebFile(params string[] segments) =>
        File.ReadAllText(Path.Combine([RepoRoot, "src", "BeeDay.Web", .. segments]));

    private static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
    }
}
