using BeeDay.Domain.Enums;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.Identity;
using Deque.AxeCore.Playwright;
using Microsoft.Extensions.Options;

namespace BeeDay.E2E.Tests;

// EPIC 28, Sprint 28.9 (Email Client Compatibility, Responsive & Accessibility QA): renders the real
// IdentityEmailComposer HTML output in a real Chromium instance - the closest automated approximation
// of layout behavior this repository can produce without a live app or route. This is explicitly an
// approximation, not evidence of Gmail/Outlook/iCloud rendering equivalence (those engines differ
// materially, especially Outlook desktop's Word-based renderer) - see the compatibility matrix in
// docs/epics/28-transactional-email-experience/README.md for what remains MANUAL/POST-MERGE PENDING.
public sealed class EmailClientCompatibilityTests(EmailPreviewPlaywrightFixture fixture) : IClassFixture<EmailPreviewPlaywrightFixture>
{
    // Matches BrandTypographyTests' own narrow-viewport convention (390/1280) elsewhere in this
    // suite - not a new, independently-chosen breakpoint.
    private const int NarrowWidth = 390;
    private const int DesktopWidth = 1280;
    private const int ViewportHeight = 900;

    public static TheoryData<string, UserLanguage, string> Flows() => new()
    {
        { "ComposeEmailConfirmation", UserLanguage.English, "Ana" },
        { "ComposeEmailConfirmation", UserLanguage.Portuguese, "Ana" },
        { "ComposePasswordReset", UserLanguage.English, "Ana" },
        { "ComposePasswordReset", UserLanguage.Portuguese, "Ana" },
        // A long display name is realistic user input (no length limit is enforced on User.Name at
        // this boundary) and pt-BR strings are generally longer than their en-US source (per the
        // Writing System's own "projetar para expansão" rule) - this combination stresses both
        // dimensions of "content the layout cannot control" at once.
        { "ComposeEmailConfirmation", UserLanguage.Portuguese, "Maria Antonieta de Souza Nascimento Rodrigues" }
    };

    [Theory]
    [MemberData(nameof(Flows))]
    public async Task Flow_RendersWithoutHorizontalOverflow_AtNarrowAndDesktopWidths(string method, UserLanguage language, string displayName)
    {
        var html = ComposeHtml(method, language, displayName);

        await AssertNoHorizontalOverflowAsync(html, NarrowWidth);
        await AssertNoHorizontalOverflowAsync(html, DesktopWidth);
    }

    [Theory]
    [MemberData(nameof(Flows))]
    public async Task Flow_HasNoAutomaticallyDetectableAccessibilityViolations(string method, UserLanguage language, string displayName)
    {
        var html = ComposeHtml(method, language, displayName);

        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.SetViewportSizeAsync(DesktopWidth, ViewportHeight);
            await page.SetContentAsync(html);

            var result = await page.RunAxe();

            // "landmark-one-main"/"region" (axe severity: moderate) fire because this is a full,
            // standalone HTML document with no <main>/<nav> ARIA landmark structure — axe's rule set
            // is calibrated for web application pages, not transactional email. ARIA landmarks are
            // not a recognized HTML-email accessibility technique (some email-client sanitizers strip
            // or mishandle them), so adding them here would not improve any real screen reader's
            // experience of an email and risks client-compatibility regressions for no benefit.
            // Explicitly excluded, not silently ignored — every other rule (including color-contrast,
            // which this Sprint's scan did catch and fix) still applies.
            var acceptedExceptions = new[] { "landmark-one-main", "region" };
            var violations = result.Violations.Where(v => !acceptedExceptions.Contains(v.Id)).ToList();
            var details = string.Join(
                Environment.NewLine,
                violations.Select(violation =>
                    $"{violation.Id} ({violation.Impact}): {string.Join(" | ", violation.Nodes.Select(node => node.Html))}"));

            Assert.False(violations.Count > 0, $"axe found violations in {method}/{language}:{Environment.NewLine}{details}");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    // "Images blocked" and "custom fonts blocked" are structurally always true for this content: no
    // <img> tag and no remote @font-face/<link> exist anywhere in the template (proven directly, not
    // by absence of network activity, which SetContentAsync wouldn't exercise anyway) - so there is
    // nothing for a client policy to block. This is the automated evidence for that matrix cell.
    [Fact]
    public void Template_NeverReferencesAnyRemoteAsset()
    {
        var html = ComposeHtml("ComposeEmailConfirmation", UserLanguage.English, "Ana");

        Assert.DoesNotContain("<img", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<link", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("@import", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fonts.googleapis.com", html, StringComparison.OrdinalIgnoreCase);
    }

    private async Task AssertNoHorizontalOverflowAsync(string html, int viewportWidth)
    {
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.SetViewportSizeAsync(viewportWidth, ViewportHeight);
            await page.SetContentAsync(html);

            var scrollWidth = await page.EvaluateAsync<int>("document.documentElement.scrollWidth");

            Assert.True(
                scrollWidth <= viewportWidth,
                $"Content overflowed horizontally at {viewportWidth}px viewport: document.documentElement.scrollWidth was {scrollWidth}px.");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static string ComposeHtml(string method, UserLanguage language, string displayName)
    {
        var composer = new IdentityEmailComposer(Options.Create(new IdentityEmailOptions
        {
            PublicBaseUrl = "https://beeday.example",
            ConfirmationPath = "/account/confirm-email",
            PasswordResetPath = "/account/reset-password"
        }));

        var message = method == "ComposeEmailConfirmation"
            ? composer.ComposeEmailConfirmation("player@example.com", displayName, "token", language)
            : composer.ComposePasswordReset("player@example.com", displayName, "token", language);

        return message.HtmlBody;
    }
}
