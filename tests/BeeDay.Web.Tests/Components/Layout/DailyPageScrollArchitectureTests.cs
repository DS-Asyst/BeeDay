using System.Text.RegularExpressions;

namespace LevelUp.Web.Tests.Components.Layout;

/// <summary>
/// Guards the single-document-scroll contract for the app shell and the Daily
/// board: <c>.levelup-content-shell</c> and <c>.dashboard-grid</c> must never
/// go back to pairing a non-<c>visible</c> overflow-x with a <c>visible</c>
/// overflow-y, because browsers silently promote that <c>visible</c> axis to
/// <c>auto</c> (per the CSS Overflow spec), turning the element back into an
/// unintended internal vertical scroll container.
///
/// This is a text-level contract check, not a computed-style assertion —
/// bUnit has no layout/rendering engine, so it cannot evaluate the actual
/// forced-overflow browser behavior this guards against. That behavior was
/// verified against the real running app (getComputedStyle + genuine
/// overflow content) during Sprint 8.2; see the design-system layout docs.
/// </summary>
public sealed class DailyPageScrollArchitectureTests
{
    private static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException("Could not locate the repository root (BeeDay.slnx) from the test output directory.");
        }

        return directory.FullName;
    }

    private static string ReadCss(params string[] relativeSegments) =>
        File.ReadAllText(Path.Combine([ResolveRepoRoot(), .. relativeSegments]));

    private static string? ExtractRuleBody(string css, string selector)
    {
        var match = Regex.Match(css, $@"{Regex.Escape(selector)}\s*{{([^}}]*)}}", RegexOptions.Singleline);
        if (!match.Success)
        {
            return null;
        }

        // Strip comments so explanatory notes about *why* a pattern was
        // removed don't themselves trip the "does not contain" assertions.
        return Regex.Replace(match.Groups[1].Value, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
    }

    [Fact]
    public void ContentShellDoesNotPairNonVisibleOverflowXWithVisibleOverflowY()
    {
        var css = ReadCss("src", "BeeDay.Web", "Components", "Layout", "MainLayout.razor.css");
        var rule = ExtractRuleBody(css, ".levelup-content-shell");

        Assert.NotNull(rule);
        Assert.DoesNotContain("overflow-x: hidden", rule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("overflow-y: auto", rule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("overflow-y: scroll", rule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("overflow: auto", rule, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DashboardGridDoesNotPairOverflowXWithVisibleOverflowY()
    {
        var css = ReadCss("src", "BeeDay.Web", "Components", "Features", "Dashboard", "Pages", "Home.razor.css");
        var rule = ExtractRuleBody(css, ".dashboard-grid");

        Assert.NotNull(rule);
        Assert.DoesNotContain("overflow-y: visible", rule, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MainLayoutDoesNotReintroduceViewportBasedMainHeight()
    {
        var css = ReadCss("src", "BeeDay.Web", "Components", "Layout", "MainLayout.razor.css");
        var rule = ExtractRuleBody(css, ".levelup-main");

        Assert.NotNull(rule);
        Assert.DoesNotContain("100vh", rule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("100dvh", rule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("overflow-y: auto", rule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("overflow-y: scroll", rule, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DashboardGridDoesNotReintroduceViewportBasedMaxHeight()
    {
        var css = ReadCss("src", "BeeDay.Web", "Components", "Features", "Dashboard", "Pages", "Home.razor.css");
        var rule = ExtractRuleBody(css, ".dashboard-grid");

        Assert.NotNull(rule);
        Assert.DoesNotContain("max-height", rule, StringComparison.OrdinalIgnoreCase);
    }
}
