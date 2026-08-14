namespace BeeDay.E2E.Tests;

/// <summary>Sprint 21.8 icon contracts verified in Chromium, including external SVG sprite use.</summary>
public sealed class IconSystemTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task OfficialLucideOutlineSpriteIsAvailableToProductConsumers()
    {
        await GotoAsync("/");

        var spriteResponse = await Page.APIRequest.GetAsync("/icons/sprite.svg");
        Assert.True(spriteResponse.Ok);
        var sprite = await spriteResponse.TextAsync();
        Assert.Contains("stroke=\"currentColor\"", sprite, StringComparison.Ordinal);
        Assert.Contains("fill=\"none\"", sprite, StringComparison.Ordinal);
        Assert.Contains("stroke-width=\"2\"", sprite, StringComparison.Ordinal);
    }
}
