using System.Text.RegularExpressions;

namespace BeeDay.Web.Tests.Components.DesignSystem;

public sealed class CoreComponentContractTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();

    [Fact]
    public void SharedPrimitiveInventoryHasOneCanonicalImplementationPerContract()
    {
        var designSystemRoot = Path.Combine(RepoRoot, "src", "BeeDay.Web", "Components", "DesignSystem");
        var actual = Directory.GetFiles(designSystemRoot, "*.razor", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Pages{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => Path.GetFileNameWithoutExtension(path)
                ?? throw new InvalidOperationException($"Component path has no file name: {path}"))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expected = new[]
        {
            "BeeDayBrand", "BeeDayButton", "BeeDayCard", "BeeDayCardMenu", "BeeDayCheckbox",
            "BeeDayConfirmDialog", "BeeDayDashboardSkeleton", "BeeDayDateInput", "BeeDayEmptyState",
            "BeeDayHero", "BeeDayIcon", "BeeDayInput", "BeeDayLoading", "BeeDayPageHeader",
            "BeeDayProgressBar", "BeeDaySectionHeader", "BeeDaySelect", "BeeDaySettingsForm",
            "BeeDaySettingsSection", "BeeDaySkeleton", "BeeDayTextArea", "BeeDayToastHost",
            "BeeDayValidationMessage", "EditorModalShell", "SearchHighlight"
        }.Order(StringComparer.Ordinal).ToArray();

        Assert.Equal(expected, actual);
        Assert.DoesNotContain(actual, name => name.Contains("V2", StringComparison.OrdinalIgnoreCase));
        Assert.True(File.Exists(Path.Combine(
            RepoRoot, "src", "BeeDay.Web", "Components", "Behaviors", "DragDrop", "BeeDaySortable.razor")));
    }

    [Fact]
    public void NativeControlInventoryStaysExplicitUntilOwningFeatureSprints()
    {
        var componentsRoot = Path.Combine(RepoRoot, "src", "BeeDay.Web", "Components");
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["button"] = 0,
            ["input"] = 0,
            ["select"] = 0,
            ["textarea"] = 0
        };
        var filesWithNativeControls = 0;

        foreach (var file in Directory.GetFiles(componentsRoot, "*.razor", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file);
            var fileContainsNativeControl = false;

            foreach (var tag in counts.Keys.ToArray())
            {
                var matches = Regex.Matches(source, $"<{tag}(?=[\\s>])", RegexOptions.IgnoreCase).Count;
                counts[tag] += matches;
                fileContainsNativeControl |= matches > 0;
            }

            filesWithNativeControls += fileContainsNativeControl ? 1 : 0;
        }

        // EPIC 27 Sprint 27.11: TagFormModal's native <input type="color"> (plus its paired
        // <InputText>, which this regex never counted) was replaced by a single looped native
        // <button> rendering the 10-swatch COR0-COR9 picker — one fewer input, one more button.
        Assert.Equal(30, counts["button"]);
        Assert.Equal(13, counts["input"]);
        Assert.Equal(0, counts["select"]);
        Assert.Equal(0, counts["textarea"]);
        Assert.Equal(43, counts.Values.Sum());
        Assert.Equal(20, filesWithNativeControls);
    }

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
