namespace BeeDay.Web.Tests.Components.Layout;

/// <summary>
/// Guards the EPIC 21 shell foundation contract established in Sprint 21.2 and evolved in Sprint
/// 21.3: MainLayout must compose the Sidebar / Main Content / Right Rail regions without dropping
/// any existing authenticated shell functionality (profile panel, account/support panel, logout
/// form, footer, toasts), and desktop/mobile navigation must never both be visible at the same
/// breakpoint — TopNavigation was fully removed in Sprint 21.3 once MobileHeader/MobileSidebar
/// absorbed its responsibilities (no two navigation systems left concurrently). See
/// docs/epics/21-lingo-product-experience/README.md §3/§4/§10/§13/§22 and "Sprint 21.3".
///
/// This is a text-level contract check, not a computed-style assertion — bUnit has no
/// layout/rendering engine, matching the approach already used by
/// <see cref="DailyPageScrollArchitectureTests"/> for the same reason.
/// </summary>
public sealed class ShellFoundationTests
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

    private static string ReadLayoutFile(string fileName) =>
        File.ReadAllText(Path.Combine(ResolveRepoRoot(), "src", "BeeDay.Web", "Components", "Layout", fileName));

    [Fact]
    public void TopNavigationNoLongerExistsInTheLayoutDirectory()
    {
        var razorPath = Path.Combine(ResolveRepoRoot(), "src", "BeeDay.Web", "Components", "Layout", "TopNavigation.razor");
        var cssPath = Path.Combine(ResolveRepoRoot(), "src", "BeeDay.Web", "Components", "Layout", "TopNavigation.razor.css");

        Assert.False(File.Exists(razorPath), "TopNavigation.razor should have been deleted once its responsibilities were fully absorbed by MobileHeader/MobileSidebar/DesktopSidebar (Sprint 21.3) — a leftover file would be dead code.");
        Assert.False(File.Exists(cssPath));
    }

    [Fact]
    public void MainLayoutComposesTheShellRegionsWithoutRemovingAnyExistingRegion()
    {
        var markup = ReadLayoutFile("MainLayout.razor");

        Assert.Contains("class=\"beeday-shell\"", markup, StringComparison.Ordinal);
        Assert.Contains("<DesktopSidebar", markup, StringComparison.Ordinal);
        Assert.Contains("<RightRail", markup, StringComparison.Ordinal);
        Assert.Contains("<MobileHeader", markup, StringComparison.Ordinal);
        Assert.Contains("<MobileSidebar", markup, StringComparison.Ordinal);

        Assert.DoesNotContain("<TopNavigation", markup, StringComparison.Ordinal);
        Assert.Contains("<ProfileSidePanel", markup, StringComparison.Ordinal);
        Assert.Contains("<AccountSidePanel", markup, StringComparison.Ordinal);
        Assert.Contains("<AppFooter", markup, StringComparison.Ordinal);
        Assert.Contains("<BeeDayToastHost", markup, StringComparison.Ordinal);
        Assert.Contains("@Body", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void DesktopSidebarIsHiddenByDefaultAndShownOnlyAtTheStructuralBreakpoint()
    {
        var css = ReadLayoutFile("DesktopSidebar.razor.css");

        Assert.Matches(@"\.desktop-sidebar\s*\{[^}]*display:\s*none", css);
        Assert.Contains("@media (min-width: 1024px)", css, StringComparison.Ordinal);
        Assert.Matches(@"@media \(min-width: 1024px\)\s*\{\s*\.desktop-sidebar\s*\{[^}]*display:\s*flex", css);
    }

    [Fact]
    public void RightRailIsHiddenByDefaultAndShownOnlyAtTheStructuralBreakpoint()
    {
        var css = ReadLayoutFile("RightRail.razor.css");

        Assert.Matches(@"\.right-rail\s*\{[^}]*display:\s*none", css);
        Assert.Contains("@media (min-width: 1024px)", css, StringComparison.Ordinal);
        Assert.Matches(@"@media \(min-width: 1024px\)\s*\{\s*\.right-rail\s*\{[^}]*display:\s*block", css);
    }

    [Fact]
    public void MobileHeaderHidesAtTheSameStructuralBreakpointTheDesktopSidebarAppearsAtSoNoTwoDesktopShellsCompete()
    {
        var css = ReadLayoutFile("MobileHeader.razor.css");

        Assert.Matches(@"@media \(min-width: 1024px\)\s*\{\s*\.mobile-header\s*\{[^}]*display:\s*none", css);
    }

    [Fact]
    public void MobileSidebarHidesAtTheSameStructuralBreakpointTheDesktopSidebarAppearsAt()
    {
        var css = ReadLayoutFile("MobileSidebar.razor.css");

        Assert.Contains("@media (min-width: 1024px)", css, StringComparison.Ordinal);
        Assert.Matches(@"@media \(min-width: 1024px\)\s*\{[^}]*\.mobile-nav-backdrop,[^}]*\.mobile-nav-drawer[^}]*\{[^}]*display:\s*none", css);
    }

    [Fact]
    public void MainLayoutReclaimsTheMobileHeaderReservedSpaceAtTheStructuralBreakpoint()
    {
        var css = ReadLayoutFile("MainLayout.razor.css");

        Assert.Matches(@"@media \(min-width: 1024px\)\s*\{\s*\.beeday-app\s*\{[^}]*--beeday-top-navigation-height:\s*0px", css);
        Assert.Contains("--beeday-sidebar-width", css, StringComparison.Ordinal);
        Assert.Contains("--beeday-right-rail-width", css, StringComparison.Ordinal);
        Assert.Contains("--beeday-content-max-width", css, StringComparison.Ordinal);
    }

    [Fact]
    public void MainContentGainsAMaxWidthWithoutReintroducingAnInternalScrollContainer()
    {
        var css = ReadLayoutFile("MainLayout.razor.css");

        Assert.Contains("max-width: var(--beeday-content-max-width)", css, StringComparison.Ordinal);
        Assert.Contains("margin-inline: auto", css, StringComparison.Ordinal);
    }
}
