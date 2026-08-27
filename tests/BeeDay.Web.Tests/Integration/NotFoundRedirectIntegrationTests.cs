using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Sprint 32.2 (EPIC 32, EXP32-F014): a nonexistent route must redirect to the real
/// <c>/not-found</c> route rather than re-execute the pipeline while keeping the broken URL in
/// place — see the <c>UseStatusCodePages</c> comment in Program.cs for why ReExecute broke Logout
/// for any authenticated user landing on a broken link (only reproducible
/// with a real interactive circuit, so <c>NavigationTests.NonexistentRoute_WhileAuthenticated_LogoutStillWorks</c>
/// is the test that actually proves the fix end to end; this one is a fast, deterministic guard
/// against the redirect itself regressing back to ReExecute or a different target).
/// </summary>
public sealed class NotFoundRedirectIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task NonexistentRoute_RedirectsToNotFoundInsteadOfReExecuting()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/a-route-that-genuinely-does-not-exist", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/not-found", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task NonexistentRoute_WhileAuthenticated_RedirectTargetCarriesAValidAntiforgeryToken()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync(
            "notfound-redirect@beeday.invalid", "Password123!", cancellationToken);

        // The redirect target itself — proven to actually be where a nonexistent route lands by
        // NonexistentRoute_RedirectsToNotFoundInsteadOfReExecuting above. Throws if the antiforgery
        // field is missing/empty — exactly the failure mode this Sprint found live.
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/not-found", cancellationToken);
        Assert.NotEmpty(token);
    }
}
