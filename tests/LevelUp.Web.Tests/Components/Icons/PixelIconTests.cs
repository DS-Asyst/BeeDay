using LevelUp.Web.Components.DesignSystem.Icons;

namespace LevelUp.Web.Tests.Components.Icons;

public sealed class PixelIconTests
{
    [Fact]
    public void RendersDecorativeIconFromSprite()
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Search));

        var svg = cut.Find("svg");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Null(svg.GetAttribute("role"));
        Assert.Equal("/icons/sprite.svg#search", cut.Find("use").GetAttribute("href"));
    }

    [Fact]
    public void RendersAccessibleLabelWhenNotDecorative()
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Warning)
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

        Assert.Throws<InvalidOperationException>(() => context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Information)
            .Add(component => component.Decorative, false)));
    }

    [Theory]
    [InlineData(PixelIconSize.ExtraSmall, "12")]
    [InlineData(PixelIconSize.Small, "16")]
    [InlineData(PixelIconSize.Medium, "20")]
    [InlineData(PixelIconSize.Large, "24")]
    [InlineData(PixelIconSize.ExtraLarge, "32")]
    public void MapsOfficialSizeTokens(PixelIconSize size, string pixels)
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Inventory)
            .Add(component => component.Size, size));

        var svg = cut.Find("svg");
        Assert.Equal(pixels, svg.GetAttribute("width"));
        Assert.Equal(pixels, svg.GetAttribute("height"));
        Assert.Contains($"pixel-icon--size-{size.ToString().ToLowerInvariant()}", svg.ClassList);
    }

    [Fact]
    public void AppliesColorAndCustomClasses()
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Inventory)
            .Add(component => component.Color, PixelIconColor.Primary)
            .Add(component => component.Class, "menu-icon"));

        var svg = cut.Find("svg");
        Assert.Contains("pixel-icon--inventory", svg.ClassList);
        Assert.Contains("pixel-icon--color-primary", svg.ClassList);
        Assert.Contains("menu-icon", svg.ClassList);
    }

    [Fact]
    public void FallsBackWithoutBreakingRenderingForUnknownName()
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, (PixelIconName)999));

        Assert.Equal("Warning", cut.Find("svg").GetAttribute("data-icon"));
        Assert.Equal("/icons/sprite.svg#warning", cut.Find("use").GetAttribute("href"));
    }
    [Theory]
    [InlineData(PixelIconName.Home, "home")]
    [InlineData(PixelIconName.Character, "character")]
    [InlineData(PixelIconName.Donate, "donate")]
    [InlineData(PixelIconName.Logout, "logout")]
    [InlineData(PixelIconName.Menu, "menu")]
    [InlineData(PixelIconName.Support, "support")]
    [InlineData(PixelIconName.Facebook, "facebook")]
    [InlineData(PixelIconName.Instagram, "instagram")]
    [InlineData(PixelIconName.YouTube, "youtube")]
    [InlineData(PixelIconName.X, "x")]
    [InlineData(PixelIconName.LinkedIn, "linkedin")]
    [InlineData(PixelIconName.GitHub, "github")]
    public void ResolvesNavigationAndSocialIcons(PixelIconName name, string symbolId)
    {
        var definition = PixelIconRegistry.Resolve(name);

        Assert.Equal(symbolId, definition.SymbolId);
        Assert.Contains($"/{symbolId}.svg", definition.AssetPath, StringComparison.Ordinal);
    }


    [Theory]
    [InlineData(PixelIconName.Success, "success")]
    [InlineData(PixelIconName.ValidationError, "validation-error")]
    [InlineData(PixelIconName.Loading, "loading")]
    [InlineData(PixelIconName.Select, "select")]
    [InlineData(PixelIconName.CheckboxUnchecked, "checkbox-unchecked")]
    [InlineData(PixelIconName.CheckboxChecked, "checkbox-checked")]
    public void ResolvesDialogAndFormIcons(PixelIconName name, string symbolId)
    {
        Assert.Equal(symbolId, PixelIconRegistry.Resolve(name).SymbolId);
    }

    [Theory]
    [InlineData(PixelIconName.Habit, "habit")]
    [InlineData(PixelIconName.RecurringTask, "recurring-task")]
    [InlineData(PixelIconName.Project, "project")]
    [InlineData(PixelIconName.Todo, "todo")]
    [InlineData(PixelIconName.Complete, "complete")]
    [InlineData(PixelIconName.Filter, "filter")]
    [InlineData(PixelIconName.Calendar, "calendar")]
    [InlineData(PixelIconName.Repeat, "repeat")]
    [InlineData(PixelIconName.Tag, "tag")]
    [InlineData(PixelIconName.Attribute, "attribute")]
    [InlineData(PixelIconName.Cancel, "cancel")]
    public void ResolvesActivityAndActionIcons(PixelIconName name, string symbolId)
    {
        Assert.Equal(symbolId, PixelIconRegistry.Resolve(name).SymbolId);
    }

    [Theory]
    [InlineData(PixelIconName.Experience, "experience")]
    [InlineData(PixelIconName.Level, "level")]
    [InlineData(PixelIconName.Wallet, "wallet")]
    [InlineData(PixelIconName.Income, "income")]
    [InlineData(PixelIconName.Expense, "expense")]
    [InlineData(PixelIconName.Statistics, "statistics")]
    [InlineData(PixelIconName.TrendUp, "trend-up")]
    [InlineData(PixelIconName.TrendDown, "trend-down")]
    [InlineData(PixelIconName.Streak, "streak")]
    [InlineData(PixelIconName.Completed, "completed")]
    [InlineData(PixelIconName.Pending, "pending")]
    public void ResolvesDashboardAndStatisticIcons(PixelIconName name, string symbolId)
    {
        var definition = PixelIconRegistry.Resolve(name);

        Assert.Equal(symbolId, definition.SymbolId);
        Assert.Equal(PixelIconCategory.Statistics, definition.Category);
    }
}

public sealed class PixelIconContractTests
{
    public static TheoryData<PixelIconColor> AllColors => new(Enum.GetValues<PixelIconColor>());

    [Theory]
    [MemberData(nameof(AllColors))]
    public void MapsEveryOfficialColorToken(PixelIconColor color)
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Information)
            .Add(component => component.Color, color));

        Assert.Contains($"pixel-icon--color-{color.ToString().ToLowerInvariant()}", cut.Find("svg").ClassList);
    }

    [Fact]
    public void DecorativeIconIsHiddenAndNeverFocusable()
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Add));

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
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Completed)
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
        var names = Enum.GetValues<PixelIconName>();

        Assert.Equal(names.Length, PixelIconRegistry.All.Count);
        Assert.All(names, name => Assert.True(PixelIconRegistry.TryGet(name, out _), $"Missing icon contract: {name}"));
        Assert.Equal(PixelIconRegistry.All.Count, PixelIconRegistry.All.Values.Select(value => value.SymbolId).Distinct().Count());
    }

    [Fact]
    public void EveryRegistryEntryUsesTheOfficialAssetRoot()
    {
        var knownProviderFolders = new[] { "material-symbols/", "devicon/", "official-brand/", "levelup-custom/" };

        foreach (var entry in PixelIconRegistry.All)
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
        foreach (var entry in PixelIconRegistry.All)
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

        foreach (var entry in PixelIconRegistry.All)
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
        var spritePath = Path.Combine(wwwroot, PixelIconRegistry.SpritePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        var spriteContent = File.ReadAllText(spritePath);

        foreach (var entry in PixelIconRegistry.All)
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
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Save)
            .AddUnmatched("data-testid", "save-icon"));

        var svg = cut.Find("svg");
        Assert.Equal("save-icon", svg.GetAttribute("data-testid"));
        Assert.Contains("pixel-icon", svg.ClassList);
        Assert.Contains("pixel-icon--save", svg.ClassList);
    }
}

/// <summary>
/// Strongly typed mirror of the icon-mapping.csv Provider column, used only by this
/// test project to validate the mapping file. Not part of the application's public
/// contract: <see cref="PixelIconDefinition"/> and <see cref="PixelIcon"/> never expose
/// provider identity to feature components — <see cref="PixelIconName"/> remains the
/// only semantic contract they use.
/// </summary>
internal enum IconProvider
{
    MaterialSymbols,
    Devicon,
    OfficialBrand,
    LevelUpCustom
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
    public void RowCountMatchesPixelIconNameCount()
    {
        var rows = ReadMappingRows();

        Assert.Equal(Enum.GetValues<PixelIconName>().Length, rows.Count);
    }

    [Fact]
    public void EveryRowDeclaresAKnownProvider()
    {
        var rows = ReadMappingRows();

        Assert.NotEmpty(rows);
        Assert.All(rows, row => Assert.True(Enum.TryParse<IconProvider>(row["Provider"], out _),
            $"'{row["Provider"]}' for {row["PixelIconName"]} is not a known IconProvider value"));
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
    [InlineData("MaterialSymbols", "material-symbols")]
    [InlineData("Devicon", "devicon")]
    [InlineData("OfficialBrand", "official-brand")]
    public void EveryRowsGeneratedAssetExistsUnderItsProviderFolder(string provider, string providerSlug)
    {
        var rows = ReadMappingRows().Where(row => row["Provider"] == provider);
        var wwwroot = IconFileLocator.ResolveWwwroot();

        Assert.All(rows, row =>
        {
            var expectedPath = Path.Combine(wwwroot, "icons", providerSlug, row["Folder"], $"{row["SymbolId"]}.svg");
            Assert.True(File.Exists(expectedPath), $"Missing generated asset for {row["PixelIconName"]}: {expectedPath}");
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
    [InlineData("Habit", "repeat")]
    [InlineData("Streak", "repeat")]
    [InlineData("Repeat", "repeat")]
    [InlineData("Account", "account_circle")]
    [InlineData("Character", "person")]
    public void ApprovedSprint81SemanticsAreMapped(string pixelIconName, string expectedSourceName)
    {
        var row = ReadMappingRows().Single(r => r["PixelIconName"] == pixelIconName);

        Assert.Equal(expectedSourceName, row["SourceName"]);
    }

    [Theory]
    [InlineData("autorenew")]
    [InlineData("loop")]
    [InlineData("sync")]
    [InlineData("refresh")]
    [InlineData("local_fire_department")]
    public void NoRemovedHabitOrStreakSourceNameIsReferenced(string removedSourceName)
    {
        var rows = ReadMappingRows();

        Assert.DoesNotContain(rows, row => row["SourceName"] == removedSourceName);
    }

    [Fact]
    public void TrendingUpIsOnlyUsedByTrendUpNeverHabitOrStreak()
    {
        var rows = ReadMappingRows();

        Assert.DoesNotContain(rows, row =>
            row["SourceName"] == "trending_up" && row["PixelIconName"] is "Habit" or "Streak" or "Repeat");
    }

    [Fact]
    public void NoOrphanedAutorenewProductionAssetRemains()
    {
        var wwwroot = IconFileLocator.ResolveWwwroot();
        var orphan = Path.Combine(wwwroot, "icons", "material-symbols", "habits", "autorenew.svg");

        Assert.False(File.Exists(orphan), $"Obsolete autorenew asset should have been removed: {orphan}");
    }
}

internal static class IconFileLocator
{
    public static string ResolveRepoRoot()
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

    public static string ResolveWwwroot() =>
        Path.Combine(ResolveRepoRoot(), "src", "LevelUp.Web", "wwwroot");
}
