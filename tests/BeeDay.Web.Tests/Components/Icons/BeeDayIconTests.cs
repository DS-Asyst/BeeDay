using BeeDay.Web.Components.DesignSystem.Icons;

namespace BeeDay.Web.Tests.Components.Icons;

public sealed class BeeDayIconTests
{
    [Fact]
    public void RendersDecorativeIconFromSprite()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, BeeDayIconName.Search));

        var svg = cut.Find("svg");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Null(svg.GetAttribute("role"));
        Assert.Equal("/icons/sprite.svg#search", cut.Find("use").GetAttribute("href"));
    }

    [Fact]
    public void RendersAccessibleLabelWhenNotDecorative()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, BeeDayIconName.Warning)
            .Add(component => component.Decorative, false)
            .Add(component => component.Label, "Warning status"));

        var svg = cut.Find("svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Warning status", svg.GetAttribute("aria-label"));
        Assert.Null(svg.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void RejectsInformativeIconWithoutLabel()
    {
        using var context = new BunitContext();

        Assert.Throws<InvalidOperationException>(() => context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, BeeDayIconName.Information)
            .Add(component => component.Decorative, false)));
    }

    [Theory]
    [InlineData(BeeDayIconSize.ExtraSmall, "12")]
    [InlineData(BeeDayIconSize.Small, "16")]
    [InlineData(BeeDayIconSize.Medium, "20")]
    [InlineData(BeeDayIconSize.Large, "24")]
    [InlineData(BeeDayIconSize.ExtraLarge, "32")]
    public void MapsOfficialSizeTokens(BeeDayIconSize size, string pixels)
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, BeeDayIconName.Wallet)
            .Add(component => component.Size, size));

        var svg = cut.Find("svg");
        Assert.Equal(pixels, svg.GetAttribute("width"));
        Assert.Equal(pixels, svg.GetAttribute("height"));
        Assert.Contains($"beeday-icon--size-{size.ToString().ToLowerInvariant()}", svg.ClassList);
    }

    [Fact]
    public void AppliesColorAndCustomClasses()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, BeeDayIconName.Wallet)
            .Add(component => component.Color, BeeDayIconColor.Primary)
            .Add(component => component.Class, "menu-icon"));

        var svg = cut.Find("svg");
        Assert.Contains("beeday-icon--wallet", svg.ClassList);
        Assert.Contains("beeday-icon--color-primary", svg.ClassList);
        Assert.Contains("menu-icon", svg.ClassList);
    }

    [Fact]
    public void FallsBackWithoutBreakingRenderingForUnknownName()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, (BeeDayIconName)999));

        Assert.Equal("Warning", cut.Find("svg").GetAttribute("data-icon"));
        Assert.Equal("/icons/sprite.svg#warning", cut.Find("use").GetAttribute("href"));
    }
    [Theory]
    [InlineData(BeeDayIconName.Profile, "profile")]
    [InlineData(BeeDayIconName.Donate, "donate")]
    [InlineData(BeeDayIconName.Logout, "logout")]
    [InlineData(BeeDayIconName.Menu, "menu")]
    [InlineData(BeeDayIconName.Support, "support")]
    [InlineData(BeeDayIconName.Facebook, "facebook")]
    [InlineData(BeeDayIconName.Instagram, "instagram")]
    [InlineData(BeeDayIconName.YouTube, "youtube")]
    [InlineData(BeeDayIconName.X, "x")]
    [InlineData(BeeDayIconName.LinkedIn, "linkedin")]
    [InlineData(BeeDayIconName.GitHub, "github")]
    public void ResolvesNavigationAndSocialIcons(BeeDayIconName name, string symbolId)
    {
        var definition = BeeDayIconRegistry.Resolve(name);

        Assert.Equal(symbolId, definition.SymbolId);
        Assert.Contains($"/{symbolId}.svg", definition.AssetPath, StringComparison.Ordinal);
    }


    [Theory]
    [InlineData(BeeDayIconName.Success, "success")]
    [InlineData(BeeDayIconName.ValidationError, "validation-error")]
    [InlineData(BeeDayIconName.Loading, "loading")]
    [InlineData(BeeDayIconName.Select, "select")]
    [InlineData(BeeDayIconName.CheckboxUnchecked, "checkbox-unchecked")]
    [InlineData(BeeDayIconName.CheckboxChecked, "checkbox-checked")]
    public void ResolvesDialogAndFormIcons(BeeDayIconName name, string symbolId)
    {
        Assert.Equal(symbolId, BeeDayIconRegistry.Resolve(name).SymbolId);
    }

    [Theory]
    [InlineData(BeeDayIconName.Habit, "habit")]
    [InlineData(BeeDayIconName.RecurringTask, "recurring-task")]
    [InlineData(BeeDayIconName.Project, "project")]
    [InlineData(BeeDayIconName.Todo, "todo")]
    [InlineData(BeeDayIconName.Complete, "complete")]
    [InlineData(BeeDayIconName.Filter, "filter")]
    [InlineData(BeeDayIconName.Calendar, "calendar")]
    [InlineData(BeeDayIconName.Repeat, "repeat")]
    [InlineData(BeeDayIconName.Tag, "tag")]
    [InlineData(BeeDayIconName.Cancel, "cancel")]
    public void ResolvesActivityAndActionIcons(BeeDayIconName name, string symbolId)
    {
        Assert.Equal(symbolId, BeeDayIconRegistry.Resolve(name).SymbolId);
    }

    [Theory]
    [InlineData(BeeDayIconName.Experience, "experience")]
    [InlineData(BeeDayIconName.Level, "level")]
    [InlineData(BeeDayIconName.Wallet, "wallet")]
    [InlineData(BeeDayIconName.Income, "income")]
    [InlineData(BeeDayIconName.Expense, "expense")]
    [InlineData(BeeDayIconName.Statistics, "statistics")]
    [InlineData(BeeDayIconName.TrendUp, "trend-up")]
    [InlineData(BeeDayIconName.TrendDown, "trend-down")]
    [InlineData(BeeDayIconName.Completed, "completed")]
    [InlineData(BeeDayIconName.Pending, "pending")]
    public void ResolvesDashboardAndStatisticIcons(BeeDayIconName name, string symbolId)
    {
        var definition = BeeDayIconRegistry.Resolve(name);

        Assert.Equal(symbolId, definition.SymbolId);
        Assert.Equal(BeeDayIconCategory.Statistics, definition.Category);
    }
}

public sealed class BeeDayIconContractTests
{
    public static TheoryData<BeeDayIconColor> AllColors => new(Enum.GetValues<BeeDayIconColor>());

    [Theory]
    [MemberData(nameof(AllColors))]
    public void MapsEveryOfficialColorToken(BeeDayIconColor color)
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, BeeDayIconName.Information)
            .Add(component => component.Color, color));

        Assert.Contains($"beeday-icon--color-{color.ToString().ToLowerInvariant()}", cut.Find("svg").ClassList);
    }

    [Fact]
    public void DecorativeIconIsHiddenAndNeverFocusable()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, BeeDayIconName.Add));

        var svg = cut.Find("svg");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Equal("false", svg.GetAttribute("focusable"));
        Assert.Null(svg.GetAttribute("aria-label"));
        Assert.Null(svg.GetAttribute("role"));
    }

    [Fact]
    public void InformativeIconHasImageSemanticsAndIsNeverFocusable()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, BeeDayIconName.Completed)
            .Add(component => component.Decorative, false)
            .Add(component => component.Label, "Completed status"));

        var svg = cut.Find("svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Completed status", svg.GetAttribute("aria-label"));
        Assert.Equal("false", svg.GetAttribute("focusable"));
        Assert.Null(svg.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void RegistryContainsEveryEnumValueExactlyOnce()
    {
        var names = Enum.GetValues<BeeDayIconName>();

        Assert.Equal(names.Length, BeeDayIconRegistry.All.Count);
        Assert.All(names, name => Assert.True(BeeDayIconRegistry.TryGet(name, out _), $"Missing icon contract: {name}"));
        Assert.Equal(BeeDayIconRegistry.All.Count, BeeDayIconRegistry.All.Values.Select(value => value.SymbolId).Distinct().Count());
    }

    [Fact]
    public void EveryRegistryEntryUsesTheOfficialAssetRoot()
    {
        var knownProviderFolders = new[] { "lucide/", "devicon/", "official-brand/" };

        foreach (var entry in BeeDayIconRegistry.All)
        {
            Assert.StartsWith("/icons/", entry.Value.AssetPath, StringComparison.Ordinal);
            Assert.EndsWith(".svg", entry.Value.AssetPath, StringComparison.Ordinal);
            Assert.DoesNotContain("..", entry.Value.AssetPath, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(entry.Value.DefaultLabel));
            Assert.Contains(knownProviderFolders, folder => entry.Value.AssetPath.StartsWith($"/icons/{folder}", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void NoRegistryEntryReferencesAnImmutableSourceLibraryOrObsoleteStreamlinePath()
    {
        foreach (var entry in BeeDayIconRegistry.All)
        {
            Assert.DoesNotContain("streamline-pixel--", entry.Value.AssetPath, StringComparison.Ordinal);
            Assert.DoesNotContain("material-symbols--", entry.Value.AssetPath, StringComparison.Ordinal);
            Assert.DoesNotContain("devicon--", entry.Value.AssetPath, StringComparison.Ordinal);
            Assert.DoesNotContain("official-brand--", entry.Value.AssetPath, StringComparison.Ordinal);
            Assert.DoesNotContain("/design/", entry.Value.AssetPath, StringComparison.Ordinal);
            Assert.DoesNotContain("/icons/streamline/", entry.Value.AssetPath, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void EveryRegistryAssetExistsOnDisk()
    {
        var wwwroot = IconFileLocator.ResolveWwwroot();

        foreach (var entry in BeeDayIconRegistry.All)
        {
            var relativePath = entry.Value.AssetPath.TrimStart('/');
            var fullPath = Path.Combine(wwwroot, relativePath.Replace('/', Path.DirectorySeparatorChar));

            Assert.True(File.Exists(fullPath), $"Missing icon asset for {entry.Key}: {fullPath}");
        }
    }

    [Fact]
    public void SpriteFileContainsExactlyOneSymbolPerRegistryEntry()
    {
        var wwwroot = IconFileLocator.ResolveWwwroot();
        var spritePath = Path.Combine(wwwroot, BeeDayIconRegistry.SpritePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        var spriteContent = File.ReadAllText(spritePath);

        foreach (var entry in BeeDayIconRegistry.All)
        {
            var occurrences = System.Text.RegularExpressions.Regex.Matches(
                spriteContent,
                $"<symbol id=\"{System.Text.RegularExpressions.Regex.Escape(entry.Value.SymbolId)}\"").Count;

            Assert.True(occurrences == 1, $"Expected exactly one <symbol> for '{entry.Value.SymbolId}' ({entry.Key}) in sprite.svg, found {occurrences}.");
        }
    }

    [Fact]
    public void PassesAdditionalAttributesWithoutReplacingOfficialClasses()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayIcon>(parameters => parameters
            .Add(component => component.Name, BeeDayIconName.Save)
            .AddUnmatched("data-testid", "save-icon"));

        var svg = cut.Find("svg");
        Assert.Equal("save-icon", svg.GetAttribute("data-testid"));
        Assert.Contains("beeday-icon", svg.ClassList);
        Assert.Contains("beeday-icon--save", svg.ClassList);
    }
}

/// <summary>
/// Strongly typed mirror of the icon-mapping.csv Provider column, used only by this
/// test project to validate the mapping file. Not part of the application's public
/// contract: <see cref="BeeDayIconDefinition"/> and <see cref="BeeDayIcon"/> never expose
/// provider identity to feature components — <see cref="BeeDayIconName"/> remains the
/// only semantic contract they use.
/// </summary>
internal enum IconProvider
{
    Lucide,
    Devicon,
    OfficialBrand
}

public sealed class IconMappingCsvTests
{
    private static List<Dictionary<string, string>> ReadMappingRows()
    {
        var csvPath = Path.Combine(IconFileLocator.ResolveRepoRoot(), "design", "icons", "catalog", "icon-mapping.csv");
        var lines = File.ReadAllLines(csvPath);
        var header = lines[0].Split(',');
        var rows = new List<Dictionary<string, string>>();

        foreach (var line in lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var values = line.Split(',');
            var row = new Dictionary<string, string>();
            for (var i = 0; i < header.Length; i++)
            {
                row[header[i]] = values[i];
            }

            rows.Add(row);
        }

        return rows;
    }

    [Fact]
    public void RowCountMatchesBeeDayIconNameCount()
    {
        var rows = ReadMappingRows();

        Assert.Equal(Enum.GetValues<BeeDayIconName>().Length, rows.Count);
    }

    [Fact]
    public void EveryRowDeclaresAKnownProvider()
    {
        var rows = ReadMappingRows();

        Assert.NotEmpty(rows);
        Assert.All(rows, row => Assert.True(Enum.TryParse<IconProvider>(row["Provider"], out _),
            $"'{row["Provider"]}' for {row["BeeDayIconName"]} is not a known IconProvider value"));
    }

    [Fact]
    public void EveryRowDeclaresLicenseMetadata()
    {
        var rows = ReadMappingRows();

        Assert.All(rows, row => Assert.False(string.IsNullOrWhiteSpace(row["License"])));
    }

    [Fact]
    public void EverySymbolIdIsUnique()
    {
        var symbolIds = ReadMappingRows().Select(row => row["SymbolId"]).ToList();

        Assert.Equal(symbolIds.Count, symbolIds.Distinct().Count());
    }

    [Theory]
    [InlineData("Lucide", "lucide")]
    [InlineData("Devicon", "devicon")]
    [InlineData("OfficialBrand", "official-brand")]
    public void EveryRowsGeneratedAssetExistsUnderItsProviderFolder(string provider, string providerSlug)
    {
        var rows = ReadMappingRows().Where(row => row["Provider"] == provider);
        var wwwroot = IconFileLocator.ResolveWwwroot();

        Assert.All(rows, row =>
        {
            var expectedPath = Path.Combine(wwwroot, "icons", providerSlug, row["Folder"], $"{row["SymbolId"]}.svg");
            Assert.True(File.Exists(expectedPath), $"Missing generated asset for {row["BeeDayIconName"]}: {expectedPath}");
        });
    }

    [Fact]
    public void NoObsoleteStreamlineRowsRemain()
    {
        var rows = ReadMappingRows();

        Assert.DoesNotContain(rows, row =>
            !Enum.TryParse<IconProvider>(row["Provider"], out _) ||
            row["SourceName"].Contains("streamline", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("Habit", "activity")]
    [InlineData("Repeat", "repeat-2")]
    [InlineData("Account", "user-round-cog")]
    [InlineData("Profile", "circle-user-round")]
    public void ApprovedSemanticsAreMapped(string iconName, string expectedSourceName)
    {
        var row = ReadMappingRows().Single(r => r["BeeDayIconName"] == iconName);

        Assert.Equal(expectedSourceName, row["SourceName"]);
    }

    [Fact]
    public void FunctionalIconsUseLucideAndBrandIconsUseApprovedProviders()
    {
        var rows = ReadMappingRows();

        Assert.All(rows.Where(row => row["Category"] != "Social"), row => Assert.Equal("Lucide", row["Provider"]));
        Assert.All(rows.Where(row => row["Category"] == "Social"), row =>
            Assert.Contains(row["Provider"], new[] { "Devicon", "OfficialBrand" }));
    }

    [Fact]
    public void NoOrphanedAutorenewProductionAssetRemains()
    {
        var wwwroot = IconFileLocator.ResolveWwwroot();
        var orphan = Path.Combine(wwwroot, "icons", "material-symbols");

        Assert.False(Directory.Exists(orphan), $"Obsolete Material Symbols folder should have been removed: {orphan}");
    }
}

internal static class IconFileLocator
{
    public static string ResolveRepoRoot()
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

    public static string ResolveWwwroot() =>
        Path.Combine(ResolveRepoRoot(), "src", "BeeDay.Web", "wwwroot");
}
