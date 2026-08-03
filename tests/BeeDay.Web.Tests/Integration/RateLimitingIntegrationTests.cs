using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Exercises the real chained login rate limiter through actual HTTP requests against
/// <see cref="RateLimitingWebApplicationFactory"/> (IP limit 6, email limit 3, 2-second window —
/// see that class for why these numbers differ from production). Each test creates its own
/// factory instance rather than sharing one via IClassFixture, so every test starts with a
/// clean, unexhausted limiter regardless of execution order — no sliding-window waits needed.
/// </summary>
public sealed class RateLimitingIntegrationTests
{
    [Fact]
    public async Task AttemptsBelowLimit_AreProcessedNormally()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new RateLimitingWebApplicationFactory();
        await factory.SeedConfirmedUserAsync("below-limit@beeday.invalid", "Password123!");

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var response = await PostLoginAsync(factory, "below-limit@beeday.invalid", "WrongPassword!", cancellationToken);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("error=invalid", response.Headers.Location!.ToString(), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task EmailLimit_BlocksFurtherAttemptsForSameNormalizedEmail()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new RateLimitingWebApplicationFactory();
        await factory.SeedConfirmedUserAsync("email-limit@beeday.invalid", "Password123!");

        // Email limit is 3; the IP limit (6) is not the constraint being tested here.
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var response = await PostLoginAsync(factory, "email-limit@beeday.invalid", "WrongPassword!", cancellationToken);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        var fourthAttempt = await PostLoginAsync(factory, "email-limit@beeday.invalid", "WrongPassword!", cancellationToken);
        Assert.Equal(HttpStatusCode.TooManyRequests, fourthAttempt.StatusCode);
    }

    [Fact]
    public async Task EmailLimit_TreatsCaseAndWhitespaceVariantsAsTheSamePartition()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new RateLimitingWebApplicationFactory();
        await factory.SeedConfirmedUserAsync("case-variant@beeday.invalid", "Password123!");

        var variants = new[]
        {
            "case-variant@beeday.invalid",
            "CASE-VARIANT@beeday.invalid",
            " case-variant@beeday.invalid "
        };

        foreach (var variant in variants)
        {
            var response = await PostLoginAsync(factory, variant, "WrongPassword!", cancellationToken);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        // A 4th attempt, in any casing, must already be blocked — proving all three counted
        // against the same partition rather than three separate ones.
        var fourthAttempt = await PostLoginAsync(factory, "Case-Variant@BeeDay.Invalid", "WrongPassword!", cancellationToken);
        Assert.Equal(HttpStatusCode.TooManyRequests, fourthAttempt.StatusCode);
    }

    [Fact]
    public async Task IpLimit_BlocksFurtherAttemptsAcrossDifferentEmails()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new RateLimitingWebApplicationFactory();

        // Six distinct emails so the (3-permit) email limiter never triggers first; only the
        // (6-permit) IP limiter can explain a rejection here, since every request shares the
        // TestServer's single loopback-like remote IP.
        for (var attempt = 0; attempt < 6; attempt++)
        {
            var response = await PostLoginAsync(factory, $"ip-limit-{attempt}@beeday.invalid", "WrongPassword!", cancellationToken);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        var seventhAttempt = await PostLoginAsync(factory, "ip-limit-6@beeday.invalid", "WrongPassword!", cancellationToken);
        Assert.Equal(HttpStatusCode.TooManyRequests, seventhAttempt.StatusCode);
    }

    [Fact]
    public async Task RateLimitResponse_IsGenericAndDoesNotRevealAccountExistence()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new RateLimitingWebApplicationFactory();
        await factory.SeedConfirmedUserAsync("exists-rate-limited@beeday.invalid", "Password123!");

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await PostLoginAsync(factory, "exists-rate-limited@beeday.invalid", "WrongPassword!", cancellationToken);
        }

        var existingRejected = await PostLoginAsync(factory, "exists-rate-limited@beeday.invalid", "WrongPassword!", cancellationToken);

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await PostLoginAsync(factory, "never-existed@beeday.invalid", "WrongPassword!", cancellationToken);
        }

        var missingRejected = await PostLoginAsync(factory, "never-existed@beeday.invalid", "WrongPassword!", cancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, existingRejected.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, missingRejected.StatusCode);
        var existingBody = await existingRejected.Content.ReadAsStringAsync(cancellationToken);
        var missingBody = await missingRejected.Content.ReadAsStringAsync(cancellationToken);
        Assert.Equal(existingBody, missingBody);
    }

    [Fact]
    public async Task ExhaustedLimiter_DoesNotAffectUnrelatedEndpoints()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new RateLimitingWebApplicationFactory();

        for (var attempt = 0; attempt < 7; attempt++)
        {
            await PostLoginAsync(factory, $"unrelated-{attempt}@beeday.invalid", "WrongPassword!", cancellationToken);
        }

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var healthResponse = await client.GetAsync("/health/live", cancellationToken);
        var loginPageResponse = await client.GetAsync("/login", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginPageResponse.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(RateLimitingWebApplicationFactory factory, string email, string password, CancellationToken cancellationToken)
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        return await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = password,
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);
    }
}
