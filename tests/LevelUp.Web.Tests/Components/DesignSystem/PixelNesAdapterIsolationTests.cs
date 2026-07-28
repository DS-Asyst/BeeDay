using System.Text.RegularExpressions;

namespace LevelUp.Web.Tests.Components.DesignSystem;

/// <summary>
/// Guards the Sprint 11.3 pixel adapter's isolation boundary: NES.css-derived
/// classes must be confined to the Design System adapter stylesheet and its
/// one approved consumer, never leak a raw "nes-" class name into Feature or
/// Page code, and never define an unsafe global/bare-element selector that
/// could contaminate forms, tables, navigation, or typography app-wide.
/// </summary>
public sealed class PixelNesAdapterIsolationTests
{
    private static readonly string[] ForbiddenBareSelectors =
    [
        "html", "body", "input", "select", "textarea", "button", "table", "th", "td",
        "label", "h1", "h2", "h3", "h4", "h5", "h6", "a", "nav", "form"
    ];

    private static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LevelUp.slnx")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException("Could not locate the repository root (LevelUp.slnx) from the test output directory.");
        }

        return directory.FullName;
    }

    private static string ReadRepoFile(params string[] relativeSegments) =>
        File.ReadAllText(Path.Combine([ResolveRepoRoot(), .. relativeSegments]));

    /// <summary>
    /// Strips explanatory /* ... */ comments and the contents of any url(...)
    /// function (the embedded SVG data URIs use path commands like "h1"/"v1"
    /// that would otherwise false-positive as bare element selectors) before
    /// running a selector-level contract check against real CSS.
    /// </summary>
    private static string StripCommentsAndUrls(string css)
    {
        var withoutComments = Regex.Replace(css, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        return Regex.Replace(withoutComments, @"url\([^)]*\)", "url()", RegexOptions.Singleline);
    }

    private static IEnumerable<string> EnumerateFeatureAndPageFiles()
    {
        var componentsRoot = Path.Combine(ResolveRepoRoot(), "src", "LevelUp.Web", "Components");

        foreach (var extension in new[] { "*.razor", "*.razor.cs", "*.razor.css" })
        {
            foreach (var file in Directory.EnumerateFiles(componentsRoot, extension, SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(componentsRoot, file);

                // "Design System" here means the DesignSystem folder itself, which
                // owns the adapter and is exempt from this Feature/Page isolation
                // guard by definition.
                if (!relative.StartsWith("DesignSystem" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                {
                    yield return file;
                }
            }
        }
    }

    [Fact]
    public void NoNesDashClassNameOutsideDesignSystem()
    {
        var offenders = EnumerateFeatureAndPageFiles()
            .Where(file => File.ReadAllText(file).Contains("nes-", StringComparison.Ordinal))
            .ToList();

        Assert.True(offenders.Count == 0,
            $"Found forbidden 'nes-' usage outside Components/DesignSystem: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void ProvenanceExcerptIsNeverLinkedFromAppRazor()
    {
        var appRazor = ReadRepoFile("src", "LevelUp.Web", "Components", "App.razor");

        Assert.DoesNotContain("nes-core.levelup-excerpt.css", appRazor, StringComparison.Ordinal);
        Assert.Contains("css/pixel-nes.css", appRazor, StringComparison.Ordinal);
    }

    [Fact]
    public void ShippedAdapterContainsNoNesDashClassSelector()
    {
        var css = ReadRepoFile("src", "LevelUp.Web", "wwwroot", "css", "pixel-nes.css");
        var selectorsOnly = StripCommentsAndUrls(css);

        Assert.DoesNotContain("nes-", selectorsOnly, StringComparison.Ordinal);
    }

    [Fact]
    public void ShippedAdapterDefinesNoUnsafeGlobalSelector()
    {
        var css = ReadRepoFile("src", "LevelUp.Web", "wwwroot", "css", "pixel-nes.css");
        var selectorsOnly = StripCommentsAndUrls(css);

        foreach (var bareSelector in ForbiddenBareSelectors)
        {
            var pattern = $@"(^|[\s,}}]){Regex.Escape(bareSelector)}(?=[\s,{{.:>#\[])";
            Assert.False(
                Regex.IsMatch(selectorsOnly, pattern, RegexOptions.Multiline),
                $"pixel-nes.css must not define a bare/global '{bareSelector}' selector.");
        }
    }

    [Fact]
    public void PixelPanelClassAppearsOnlyInApprovedConsumer()
    {
        var componentsRoot = Path.Combine(ResolveRepoRoot(), "src", "LevelUp.Web", "Components");
        var consumers = Directory.EnumerateFiles(componentsRoot, "*.razor", SearchOption.AllDirectories)
            .Where(file => File.ReadAllText(file).Contains("levelup-pixel-panel", StringComparison.Ordinal))
            .Select(file => Path.GetFileName(file))
            .ToList();

        Assert.Equal(["LevelUpFeedbackModal.razor"], consumers);
    }

    [Fact]
    public void PixelCtaClassAppearsOnlyInApprovedConsumer()
    {
        var componentsRoot = Path.Combine(ResolveRepoRoot(), "src", "LevelUp.Web", "Components");
        var consumers = Directory.EnumerateFiles(componentsRoot, "*.razor", SearchOption.AllDirectories)
            .Where(file => File.ReadAllText(file).Contains("levelup-pixel-cta", StringComparison.Ordinal))
            .Select(file => Path.GetFileName(file))
            .ToList();

        Assert.Equal(["LevelUpFeedbackModal.razor"], consumers);
    }
}
