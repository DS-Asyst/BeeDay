namespace BeeDay.Web.Tests.Components.Layout;

public sealed class ShellFoundationTests
{
    private static string Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
            {
                directory = directory.Parent;
            }
            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }
    }

    private static string Layout(string file) => File.ReadAllText(Path.Combine(Root, "src", "BeeDay.Web", "Components", "Layout", file));

    [Fact]
    public void AuthenticatedShellContainsOnlyNavigationWorkspaceAndToastAsPermanentRegions()
    {
        var markup = Layout("MainLayout.razor");
        Assert.Contains("<DesktopSidebar", markup, StringComparison.Ordinal);
        Assert.Contains("class=\"beeday-workspace\"", markup, StringComparison.Ordinal);
        Assert.Contains("<MobileHeader", markup, StringComparison.Ordinal);
        Assert.Contains("<MobileSidebar", markup, StringComparison.Ordinal);
        Assert.Contains("<BeeDayToastHost", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("<RightRail", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("<ProfileSidePanel", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("<AccountSidePanel", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("<AppFooter", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void RetiredShellRegionsHaveNoDeadComponentFiles()
    {
        foreach (var component in new[] { "RightRail", "ProfileSidePanel", "AccountSidePanel" })
        {
            Assert.False(File.Exists(Path.Combine(Root, "src", "BeeDay.Web", "Components", "Layout", $"{component}.razor")));
            Assert.False(File.Exists(Path.Combine(Root, "src", "BeeDay.Web", "Components", "Layout", $"{component}.razor.css")));
        }
    }

    [Fact]
    public void ShellUsesOnePredictableDesktopBreakpointAndSemanticWidths()
    {
        var layoutCss = Layout("MainLayout.razor.css");
        Assert.Contains("--beeday-sidebar-width", layoutCss, StringComparison.Ordinal);
        Assert.DoesNotContain("--beeday-reading-width", layoutCss, StringComparison.Ordinal);
        Assert.DoesNotContain("--beeday-workspace-width", layoutCss, StringComparison.Ordinal);
        Assert.DoesNotContain("right-rail", layoutCss, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("panel-width", layoutCss, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@media (min-width: 1200px)", layoutCss, StringComparison.Ordinal);
        Assert.Contains("@media (min-width: 1200px)", Layout("DesktopSidebar.razor.css"), StringComparison.Ordinal);
        Assert.Contains("@media (min-width: 1200px)", Layout("MobileHeader.razor.css"), StringComparison.Ordinal);
        Assert.Contains("@media (min-width: 1200px)", Layout("MobileSidebar.razor.css"), StringComparison.Ordinal);
        Assert.DoesNotContain("1024px", layoutCss, StringComparison.Ordinal);
        Assert.DoesNotContain("1024px", Layout("DesktopSidebar.razor.css"), StringComparison.Ordinal);
        Assert.DoesNotContain("1024px", Layout("MobileHeader.razor.css"), StringComparison.Ordinal);
        Assert.DoesNotContain("1024px", Layout("MobileSidebar.razor.css"), StringComparison.Ordinal);
    }

    [Fact]
    public void PublicLayoutStillOwnsTheInstitutionalFooter()
    {
        Assert.Contains("<AppFooter", Layout("PublicLayout.razor"), StringComparison.Ordinal);
    }
}
