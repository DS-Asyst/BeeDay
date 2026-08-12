using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// The public Home (<c>/</c>) as a real anonymous visitor sees it in a real browser — Sprint 20.5
/// replaced the old anonymous-to-/login redirect with an actual public landing page.
/// </summary>
public sealed class HomeTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task AnonymousVisitor_SeesHomeWithoutRedirect()
    {
        await GotoAsync("/");

        await Expect(Page).ToHaveURLAsync(new Regex(@"/$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Be better every day", Level = 1 })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AnonymousVisitor_GetStartedCtaReachesLogin()
    {
        await GotoAsync("/");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Get started" }).First.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/login$"));
    }
}
