using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BeeDay.Web.Tests.Localization;

public sealed class ResourceCatalogContractTests
{
    private static readonly Regex PlaceholderPattern = new(
        @"\{\d+(?:,[^}:]+)?(?::[^}]+)?\}",
        RegexOptions.CultureInvariant);

    [Fact]
    public void Catalogs_HaveNeutralEnglishAndPortugueseKeyParity()
    {
        foreach (var catalog in FindCatalogs())
        {
            var neutral = ReadCatalog(catalog.NeutralPath);
            var english = ReadCatalog(catalog.EnglishPath);
            var portuguese = ReadCatalog(catalog.PortuguesePath);

            AssertSameKeys(catalog.Name, "en-US", neutral, english);
            AssertSameKeys(catalog.Name, "pt-BR", neutral, portuguese);
        }
    }

    [Fact]
    public void NeutralCatalogs_MatchTheDefaultEnglishCulture()
    {
        foreach (var catalog in FindCatalogs())
        {
            var neutral = ReadCatalog(catalog.NeutralPath);
            var english = ReadCatalog(catalog.EnglishPath);

            foreach (var (key, neutralValue) in neutral)
            {
                Assert.True(
                    string.Equals(neutralValue, english[key], StringComparison.Ordinal),
                    $"{catalog.Name}:{key} must match en-US because en-US is the default culture.");
            }
        }
    }

    [Fact]
    public void LocalizedValues_PreserveCompositeFormatPlaceholders()
    {
        foreach (var catalog in FindCatalogs())
        {
            var english = ReadCatalog(catalog.EnglishPath);
            var portuguese = ReadCatalog(catalog.PortuguesePath);

            foreach (var (key, englishValue) in english)
            {
                var englishPlaceholders = FindPlaceholders(englishValue);
                var portuguesePlaceholders = FindPlaceholders(portuguese[key]);

                Assert.True(
                    englishPlaceholders.SequenceEqual(portuguesePlaceholders, StringComparer.Ordinal),
                    $"{catalog.Name}:{key} has different placeholders in en-US and pt-BR.");
            }
        }
    }

    private static void AssertSameKeys(
        string catalogName,
        string culture,
        IReadOnlyDictionary<string, string> neutral,
        IReadOnlyDictionary<string, string> localized)
    {
        var neutralKeys = neutral.Keys.Order(StringComparer.Ordinal);
        var localizedKeys = localized.Keys.Order(StringComparer.Ordinal);

        Assert.True(
            neutralKeys.SequenceEqual(localizedKeys, StringComparer.Ordinal),
            $"{catalogName} does not have key parity between neutral and {culture} resources.");
    }

    private static string[] FindPlaceholders(string value) =>
        PlaceholderPattern.Matches(value)
            .Select(match => match.Value)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyDictionary<string, string> ReadCatalog(string path) =>
        XDocument.Load(path)
            .Root!
            .Elements("data")
            .ToDictionary(
                element => element.Attribute("name")!.Value,
                element => element.Element("value")?.Value ?? string.Empty,
                StringComparer.Ordinal);

    private static IReadOnlyList<ResourceCatalog> FindCatalogs()
    {
        var webRoot = Path.Combine(ResolveRepoRoot(), "src", "BeeDay.Web");
        var neutralPaths = Directory.GetFiles(webRoot, "*.resx", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".en-US.resx", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".pt-BR.resx", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(neutralPaths);

        return neutralPaths.Select(path =>
        {
            var stem = path[..^".resx".Length];
            return new ResourceCatalog(
                Path.GetFileName(stem),
                path,
                $"{stem}.en-US.resx",
                $"{stem}.pt-BR.resx");
        }).ToArray();
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

    private sealed record ResourceCatalog(
        string Name,
        string NeutralPath,
        string EnglishPath,
        string PortuguesePath);
}
