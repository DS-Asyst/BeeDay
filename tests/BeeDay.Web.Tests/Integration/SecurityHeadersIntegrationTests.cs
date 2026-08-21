namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Verifies the security response headers this app ACTUALLY sends today, rather than assuming from
/// reading Program.cs. Two sources contribute headers: Program.cs (<c>UseHsts()</c>,
/// Production-only, plus <c>SecurityHeadersMiddleware</c> since EPIC 30 Sprint 30.22 —
/// X-Content-Type-Options, Referrer-Policy, Permissions-Policy) and ASP.NET Core's Razor Components
/// framework, which — independent of anything this app configures — automatically adds
/// <c>Content-Security-Policy: frame-ancestors 'self'</c>, <c>X-Frame-Options: SAMEORIGIN</c>, and
/// no-cache Cache-Control/Pragma headers to every interactive-server-rendered response.
/// docs/security/01-security-baseline.md lists a fuller, script-src-covering CSP as still planned —
/// deliberately not attempted here (see the middleware's own doc comment for why).
/// </summary>
/// <remarks>
/// HSTS could not be verified through <see cref="ProductionLikeWebApplicationFactory"/>:
/// <c>HstsMiddleware</c> only adds its header when <c>HttpContext.Request.IsHttps</c> is true, and
/// <c>Microsoft.AspNetCore.Mvc.Testing</c>'s in-memory TestServer never performs a real TLS
/// handshake — an https:// <c>BaseAddress</c> only affects how relative URLs are built, not
/// <c>Request.IsHttps</c>. This was confirmed by direct observation (a captured Production response
/// showed no Strict-Transport-Security header, status 200, everything else present as expected) and
/// is a known limitation of testing HSTS via WebApplicationFactory in general, not a defect in
/// Program.cs — see the final sprint report.
/// </remarks>
public sealed class SecurityHeadersIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task LoginPage_IncludesFrameworkProvidedFrameAncestorsAndClickjackingHeaders()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/login", cancellationToken);

        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var csp));
        Assert.Contains(csp, value => value.Contains("frame-ancestors 'self'", StringComparison.Ordinal));

        Assert.True(response.Headers.TryGetValues("X-Frame-Options", out var frameOptions));
        Assert.Contains(frameOptions, value => value.Equals("SAMEORIGIN", StringComparison.Ordinal));

        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.NoCache);
    }

    // EPIC 30 Sprint 30.22: SecurityHeadersMiddleware now sends all three. Deliberately does not
    // touch X-Frame-Options/CSP — the framework already owns those (see the class comment above and
    // LoginPage_IncludesFrameworkProvidedFrameAncestorsAndClickjackingHeaders, still passing
    // unchanged, confirming no conflict was introduced).
    [Fact]
    public async Task LoginPage_SendsReferrerPolicyContentTypeOptionsAndPermissionsPolicy()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/login", cancellationToken);

        Assert.True(response.Headers.TryGetValues("Referrer-Policy", out var referrerPolicy));
        Assert.Contains(referrerPolicy, value => value.Equals("strict-origin-when-cross-origin", StringComparison.Ordinal));

        Assert.True(response.Headers.TryGetValues("X-Content-Type-Options", out var contentTypeOptions));
        Assert.Contains(contentTypeOptions, value => value.Equals("nosniff", StringComparison.Ordinal));

        Assert.True(response.Headers.TryGetValues("Permissions-Policy", out var permissionsPolicy));
        Assert.Contains(permissionsPolicy, value => value.Equals("camera=(), microphone=(), geolocation=()", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Development_ResponseDoesNotIncludeStrictTransportSecurityHeader()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/login", cancellationToken);

        // UseHsts() only runs for non-Development (Program.cs) — this at least confirms it is
        // never sent in Development, the one HSTS-related fact TestServer can verify here.
        Assert.False(response.Headers.Contains("Strict-Transport-Security"));
    }
}
