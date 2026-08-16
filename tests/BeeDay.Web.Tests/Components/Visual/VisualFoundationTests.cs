namespace BeeDay.Web.Tests.Components.Visual;

/// <summary>
/// Guards the global visual foundation established by Epic 21 Sprint 21.4. These are source-level
/// contracts because bUnit does not compute linked stylesheets or download web fonts.
/// </summary>
public sealed class VisualFoundationTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();

    [Fact]
    public void BrandFamilyUsesTheOfficialPrimaryPaletteWithoutASecondBrandColor()
    {
        var css = ReadWebFile("wwwroot", "css", "variables.css");

        Assert.Contains("--beeday-color-brand-primary: #5247f9;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-hover: #3f33f1;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-active: #1c0ef2;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-light: #827afc;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-soft: #f8f7ff;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-yellow: var(--beeday-color-reward);", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-yellow-hover: var(--beeday-color-reward-hover);", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-yellow-foreground: var(--beeday-color-reward-foreground);", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#1023c8", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#1e33ed", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#0c1b99", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#3044d6", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--beeday-game-yellow", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--lingo-", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--epic21-", css, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublicHomeAcquisitionCtaUsesTheSharedBrandPrimaryTokenWithoutAParallelColor()
    {
        var variables = ReadWebFile("wwwroot", "css", "variables.css");
        var home = ReadWebFile("Components", "Features", "Home", "Pages", "Home.razor.css");

        Assert.DoesNotContain("public-home-cta", variables, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("public-home-cta", home, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--beeday-button-background", home, StringComparison.Ordinal);
        Assert.DoesNotContain("#14adff", home, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#2cbaff", home, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#0079b9", home, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SharedBrandUsesTheOfficialPrimaryInEverySupportedMode()
    {
        var brand = ReadWebFile("Components", "DesignSystem", "Text", "BeeDayBrand.razor.css");

        Assert.Contains(".beeday-brand__bee,", brand, StringComparison.Ordinal);
        Assert.Contains(".beeday-brand__day { color: var(--beeday-color-brand-primary); }", brand, StringComparison.Ordinal);
        Assert.Contains(".beeday-brand--inverse .beeday-brand__bee,", brand, StringComparison.Ordinal);
        Assert.Contains(".beeday-brand--inverse .beeday-brand__day { color: var(--beeday-color-brand-primary); }", brand, StringComparison.Ordinal);
        Assert.DoesNotContain("--beeday-color-brand-yellow", brand, StringComparison.Ordinal);
        Assert.DoesNotContain("--beeday-color-text-inverse", brand, StringComparison.Ordinal);
    }

    [Fact]
    public void RewardAndButtonAliasesPreserveSemanticOwnership()
    {
        var variables = ReadWebFile("wwwroot", "css", "variables.css");
        var progress = ReadWebFile("Components", "DesignSystem", "Progress", "BeeDayProgressBar.razor.css");
        var experience = ReadWebFile("Components", "Features", "Experience", "Components", "ExperienceBar.razor.css");

        Assert.Contains("--beeday-color-reward: #ffd326;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-reward-active: #cda600;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-reward-foreground: var(--beeday-color-text-primary);", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-color-button-success-fg: var(--beeday-color-text-inverse);", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-color-button-danger-fg: var(--beeday-color-text-inverse);", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-color-button-confirmation-cancel-bg: var(--beeday-color-surface);", variables, StringComparison.Ordinal);
        Assert.Contains("var(--beeday-color-reward)", progress, StringComparison.Ordinal);
        Assert.Contains("var(--beeday-color-reward)", experience, StringComparison.Ordinal);
        Assert.DoesNotContain("var(--beeday-color-brand-yellow", progress, StringComparison.Ordinal);
        Assert.DoesNotContain("var(--beeday-color-brand-yellow", experience, StringComparison.Ordinal);
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
