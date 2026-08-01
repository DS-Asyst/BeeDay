using System.Globalization;
using System.Security.Claims;
using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Identity;
using LevelUp.Application.Common.Security;
using LevelUp.Domain.Entities;
using LevelUp.Web.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LevelUp.Web.Tests.Integration;

/// <summary>
/// Boots the real application (Development environment, so the production-only startup guards
/// in Program.cs don't apply) against an isolated temp JSON storage directory, so integration
/// tests never touch the developer's real local data and never collide with each other. Shared
/// by every integration test class in this project — do not duplicate this setup.
/// </summary>
/// <remarks>
/// All requests from <see cref="WebApplicationFactory{TEntryPoint}"/>'s in-memory TestServer
/// share one loopback-like remote IP, so every login attempt across every test in a class hits
/// the SAME rate-limiter IP partition. The default here is deliberately generous — high enough
/// that ordinary functional tests (login, logout, sessions, authorization, ...) never trip it —
/// so only <see cref="RateLimitingWebApplicationFactory"/>, used exclusively by the tests that
/// deliberately exhaust the limiter, overrides it with tight numbers and a short window.
/// </remarks>
public class LevelUpWebApplicationFactory : WebApplicationFactory<Program>
{
    public string StorageDirectory { get; } =
        Path.Combine(Path.GetTempPath(), "levelup-web-tests", Guid.NewGuid().ToString("N"));

    protected virtual IReadOnlyDictionary<string, string?> RateLimiterConfiguration { get; } = new Dictionary<string, string?>
    {
        // Generous enough that no functional test accidentally trips it; production values
        // (10 IP / 5 email per 1-minute window) are irrelevant here since nothing in these
        // classes deliberately exhausts the limiter.
        [$"{LoginRateLimiterOptions.SectionName}:IpPermitLimit"] = "1000",
        [$"{LoginRateLimiterOptions.SectionName}:EmailPermitLimit"] = "1000",
        [$"{LoginRateLimiterOptions.SectionName}:Window"] = "00:00:01",
        [$"{LoginRateLimiterOptions.SectionName}:SegmentsPerWindow"] = "1"
    };

    /// <summary>Hosting environment name. Override for scenarios that need Production-only behavior (e.g. cookie Secure policy).</summary>
    protected virtual string EnvironmentName => "Development";

    /// <summary>Extra configuration merged in on top of storage isolation and rate-limiter settings.</summary>
    protected virtual IReadOnlyDictionary<string, string?> AdditionalConfiguration { get; } = new Dictionary<string, string?>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["LevelUp:Storage:Directory"] = StorageDirectory,
                ["LevelUp:Email:Development:Enabled"] = "false"
            };
            foreach (var (key, value) in RateLimiterConfiguration)
            {
                settings[key] = value;
            }
            foreach (var (key, value) in AdditionalConfiguration)
            {
                settings[key] = value;
            }

            config.AddInMemoryCollection(settings);
        });
    }

    /// <summary>Creates and persists a confirmed, active User with a known password, bypassing HTTP.</summary>
    public Task<User> SeedConfirmedUserAsync(string email, string password, string name = "Test User") =>
        SeedUserAsync(email, password, name, confirmEmail: true);

    /// <summary>Creates and persists an active User whose email is NOT confirmed yet.</summary>
    public Task<User> SeedUnconfirmedUserAsync(string email, string password, string name = "Test User") =>
        SeedUserAsync(email, password, name, confirmEmail: false);

    private async Task<User> SeedUserAsync(string email, string password, string name, bool confirmEmail)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILevelUpRepository>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        // The nickname must be unique per User.CompleteUserProfile; derive it from the (always
        // distinct, per test) email rather than the shared default name to avoid collisions
        // across the multiple users seeded into one IClassFixture-shared factory. Truncated to
        // Nickname.MaximumLength (20) with a hash suffix when the local-part alone would exceed it.
        var rawNickname = new string(email
            .Split('@')[0]
            .Where(char.IsLetterOrDigit)
            .ToArray())
            .ToLowerInvariant();
        var nickname = rawNickname.Length <= LevelUp.Domain.ValueObjects.Nickname.MaximumLength
            ? rawNickname
            : rawNickname[..14] + Math.Abs(rawNickname.GetHashCode()).ToString(CultureInfo.InvariantCulture).PadLeft(6, '0')[..6];

        User? user = null;
        await repository.UpdateAsync(data =>
        {
            user = User.Create(name, email, passwordService.Hash(password));
            if (confirmEmail)
            {
                user.ConfirmEmail(user.CreatedAtUtc);
            }

            data.AddUser(user);
            data.CompleteUserProfile(user.Id, nickname);
        });

        return user!;
    }

    public async Task<User?> FindUserAsync(string email)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILevelUpRepository>();
        var data = await repository.LoadAsync();
        return data.Users.FirstOrDefault(candidate => string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILevelUpRepository>();
        await repository.UpdateAsync(data => data.FindUser(userId).SetActive(false));
    }

    /// <summary>
    /// Seeds a confirmed, active User and logs in via the real HTTP pipeline (antiforgery token
    /// fetched from a real rendered page, real /auth/login endpoint, real cookie issuance),
    /// returning the now-authenticated client.
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password, CancellationToken cancellationToken, bool allowAutoRedirect = false)
    {
        await SeedConfirmedUserAsync(email, password);

        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = allowAutoRedirect });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = password,
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);

        // /auth/login redirects on both success and invalid-credentials failure; only the
        // Location distinguishes them (…?error=invalid on failure). Fail loudly here rather than
        // silently returning an unauthenticated client that later tests would misinterpret.
        if (response.StatusCode != System.Net.HttpStatusCode.Redirect
            || (response.Headers.Location?.ToString().Contains("error=invalid", StringComparison.Ordinal) ?? false))
        {
            throw new InvalidOperationException(
                $"Expected login for '{email}' to succeed, got {response.StatusCode} -> {response.Headers.Location}.");
        }

        return client;
    }

    /// <summary>
    /// Resolves a real, scoped <see cref="ICurrentUserContext"/> (via the production
    /// <c>HttpCurrentUserContext</c> reading a manually-authenticated <see cref="HttpContext"/>)
    /// for feature flows only reachable via MediatR/Blazor, never via a raw HTTP endpoint. This
    /// exercises the real handler, the real CurrentUserGuard, and the real repository — the
    /// ICurrentUserContext interface itself is never faked or mocked.
    /// </summary>
    public IServiceScope CreateAuthenticatedScope(Guid userId, int sessionVersion)
    {
        var scope = Services.CreateScope();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(LevelUpClaimTypes.SessionVersion, sessionVersion.ToString(CultureInfo.InvariantCulture))
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext
        {
            User = principal,
            RequestServices = scope.ServiceProvider
        };

        return scope;
    }

    /// <summary>
    /// Issues a real, hashed password-reset token for the given user directly through the
    /// repository, using the app's own <see cref="IUserTokenService"/> and <see cref="IClock"/> —
    /// the same primitives <c>RequestPasswordResetCommandHandler</c> uses. This is an arrange-only
    /// shortcut: it skips the request/email-issuing step (covered separately, end-to-end, by the
    /// password-reset flow tests) so session-invalidation tests can focus on what happens once a
    /// reset token is actually consumed.
    /// </summary>
    public Task<string> IssuePasswordResetTokenAsync(Guid userId, bool expired = false) =>
        IssueTokenAsync(userId, LevelUp.Domain.Enums.UserTokenType.PasswordReset, expired);

    public Task<string> IssueEmailConfirmationTokenAsync(Guid userId, bool expired = false) =>
        IssueTokenAsync(userId, LevelUp.Domain.Enums.UserTokenType.EmailConfirmation, expired);

    private async Task<string> IssueTokenAsync(Guid userId, LevelUp.Domain.Enums.UserTokenType type, bool expired)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILevelUpRepository>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IUserTokenService>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var rawToken = tokenService.GenerateToken();
        await repository.UpdateAsync(data =>
        {
            var now = clock.UtcNow;
            var createdAt = expired ? now.AddHours(-2) : now;
            var expiresAt = expired ? now.AddHours(-1) : now.AddHours(1);
            data.AddUserToken(LevelUp.Domain.Entities.UserToken.Create(userId, type, tokenService.HashToken(rawToken), createdAt, expiresAt));
        });

        return rawToken;
    }

    /// <summary>
    /// Produces a validly-encrypted "LevelUp.Auth" cookie value for an arbitrary claim set, using
    /// the app's own real <c>CookieAuthenticationOptions.TicketDataFormat</c> — the same
    /// DataProtection-backed format /auth/login itself uses to sign cookies. This lets tests forge
    /// exactly the edge-case ticket needed (missing/invalid/stale SessionVersion claim, nonexistent
    /// user, ...) while still sending it through the REAL OnValidatePrincipal on the next request;
    /// nothing about the authentication mechanism itself is faked or bypassed.
    /// </summary>
    public string CreateRawAuthCookie(IEnumerable<Claim> claims)
    {
        var options = Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        var ticket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        return options.TicketDataFormat.Protect(ticket);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(StorageDirectory))
        {
            try
            {
                Directory.Delete(StorageDirectory, recursive: true);
            }
            catch (IOException)
            {
                // Best-effort cleanup; a stray temp folder under %TEMP% is not worth failing the test run over.
            }
        }
    }
}
