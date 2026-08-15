using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Validates route-level authorization as actually enforced by the real ASP.NET Core pipeline
/// (Blazor's <c>AuthorizeRouteView</c> for pages, <c>RequireAuthorization()</c> for minimal API
/// endpoints) — not by reading the [Authorize]/[AllowAnonymous] attributes and assuming. Covers:
/// anonymous rejection from every protected page, authenticated access to those same pages, that
/// every public page and health endpoint remains reachable anonymously, and that an unauthenticated
/// request to an authorization-gated minimal API endpoint is rejected before antiforgery is even
/// evaluated (CSRF-specific behavior is covered separately by AntiforgeryIntegrationTests).
/// </summary>
public sealed class AuthorizationIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    public static IEnumerable<object[]> ProtectedPages { get; } =
    [
        ["/profile"],
        ["/daily"],
        ["/wallet"],
        ["/account"],
        ["/onboarding/tutorial"],
        ["/design-system/icons"],
        ["/design-system/hero"]
    ];

    public static IEnumerable<object[]> PublicPages { get; } =
    [
        ["/"],
        ["/login"],
        ["/welcome"],
        ["/profile/create"],
        ["/account/forgot-password"],
        ["/account/resend-confirmation"],
        ["/account/reset-password"],
        ["/account/confirm-email"],
        ["/account/email-confirmation-sent"],
        ["/not-found"]
    ];

    [Theory]
    [MemberData(nameof(ProtectedPages))]
    public async Task Anonymous_CannotAccessProtectedPage(string path)
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync(path, cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login", response.Headers.Location!.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(ProtectedPages))]
    public async Task Authenticated_CanAccessProtectedPage(string path)
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync(
            $"authz-ok-{Guid.NewGuid():N}@beeday.invalid", "Password123!", cancellationToken);

        var response = await client.GetAsync(path, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Authenticated_HomeCompatibilityRouteRedirectsToProfile()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync(
            $"authz-home-{Guid.NewGuid():N}@beeday.invalid", "Password123!", cancellationToken);

        var response = await client.GetAsync("/home", cancellationToken);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/profile", response.Headers.Location?.AbsolutePath);
    }

    [Theory]
    [MemberData(nameof(PublicPages))]
    public async Task Anonymous_CanAccessPublicPage(string path)
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync(path, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    [InlineData("/health")]
    public async Task Anonymous_CanReachHealthEndpoints(string path)
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync(path, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_PostToAuthGatedEndpoint_IsRejectedWithoutReachingAntiforgery()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // No antiforgery token attached at all: if authorization ran after antiforgery, this
        // would fail with a 400 (malformed request) instead of an authentication challenge.
        var response = await client.PostAsync("/auth/logout", new FormUrlEncodedContent([]), cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login", response.Headers.Location!.ToString(), StringComparison.Ordinal);
    }
}
