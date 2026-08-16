using System.Globalization;
using System.Text.RegularExpressions;

namespace BeeDay.Web.Tests.Components.Visual;

/// <summary>
/// Protects text and interaction color pairs owned by the shared token contract. Runtime axe scans
/// complement these calculations for rendered consumers; raster illustrations are intentionally
/// outside this source-level contract.
/// </summary>
public sealed class DesignSystemContrastTests
{
    private static readonly Regex TokenPattern = new(
        @"--(?<name>[a-z0-9-]+):\s*(?<value>#[0-9a-f]{6}|var\(--[a-z0-9-]+\));",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly IReadOnlyDictionary<string, string> Tokens = ReadTokens();

    [Theory]
    [InlineData("beeday-color-brand-primary", "beeday-color-surface", 4.5)]
    [InlineData("beeday-color-text-primary", "beeday-color-surface", 4.5)]
    [InlineData("beeday-color-text-secondary", "beeday-color-surface", 4.5)]
    [InlineData("beeday-color-button-primary-fg", "beeday-color-button-primary-bg", 4.5)]
    [InlineData("beeday-color-button-success-fg", "beeday-color-button-success-bg", 4.5)]
    [InlineData("beeday-color-button-warning-fg", "beeday-color-button-warning-bg", 4.5)]
    [InlineData("beeday-color-button-danger-fg", "beeday-color-button-danger-bg", 4.5)]
    [InlineData("beeday-color-info", "beeday-color-info-soft", 4.5)]
    [InlineData("beeday-focus-color-inverse", "beeday-color-brand-primary", 3.0)]
    public void CoreTokenPair_MeetsItsContrastThreshold(string foregroundToken, string backgroundToken, double minimumRatio)
    {
        var foreground = ResolveHex(foregroundToken);
        var background = ResolveHex(backgroundToken);
        var ratio = ContrastRatio(foreground, background);

        Assert.True(
            ratio >= minimumRatio,
            $"--{foregroundToken} ({foreground}) on --{backgroundToken} ({background}) has {ratio:F2}:1 contrast; expected at least {minimumRatio:F1}:1.");
    }

    private static IReadOnlyDictionary<string, string> ReadTokens()
    {
        var path = Path.Combine(ResolveRepoRoot(), "src", "BeeDay.Web", "wwwroot", "css", "variables.css");
        var css = File.ReadAllText(path);

        return TokenPattern.Matches(css).ToDictionary(
            match => match.Groups["name"].Value,
            match => match.Groups["value"].Value,
            StringComparer.Ordinal);
    }

    private static string ResolveHex(string tokenName)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);

        while (true)
        {
            Assert.True(visited.Add(tokenName), $"Circular CSS token reference found at --{tokenName}.");
            Assert.True(Tokens.TryGetValue(tokenName, out var value), $"CSS token --{tokenName} was not found.");

            if (value.StartsWith('#'))
            {
                return value;
            }

            tokenName = value[6..^1];
        }
    }

    private static double ContrastRatio(string firstHex, string secondHex)
    {
        var first = RelativeLuminance(firstHex);
        var second = RelativeLuminance(secondHex);
        return (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
    }

    private static double RelativeLuminance(string hex)
    {
        var channels = new[] { hex[1..3], hex[3..5], hex[5..7] }
            .Select(channel => int.Parse(channel, NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d)
            .Select(channel => channel <= .04045
                ? channel / 12.92
                : Math.Pow((channel + .055) / 1.055, 2.4))
            .ToArray();

        return .2126 * channels[0] + .7152 * channels[1] + .0722 * channels[2];
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
