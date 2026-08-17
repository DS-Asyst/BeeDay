using System.Globalization;
using System.Security.Claims;
using BeeDay.Application.Common.Contracts;
using BeeDay.Application.Common.Identity;
using BeeDay.Application.Common.Security;
using BeeDay.Domain.Entities;
using BeeDay.Infrastructure.Persistence.SqlServer;
using BeeDay.Web.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Boots the real application (Development environment, so the production-only startup guards
/// in Program.cs don't apply) against an isolated, disposable SQL Server LocalDB database (one per
/// factory instance, migrated on startup and dropped on <see cref="Dispose"/> — the Sprint 14.6 SQL
/// Server cutover replaced the JSON temp-directory isolation this class used before), so integration
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
public class BeeDayWebApplicationFactory : WebApplicationFactory<Program>
{
    public string ConnectionString { get; } =
        $"Server=(localdb)\\mssqllocaldb;Database=BeeDay_WebTests_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True;";

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

    /// <summary>Extra configuration merged in on top of database isolation and rate-limiter settings.</summary>
    protected virtual IReadOnlyDictionary<string, string?> AdditionalConfiguration { get; } = new Dictionary<string, string?>();

    /// <summary>
    /// Whether <see cref="CreateHost"/> should acquire <see cref="ProductionEnvironmentVariableTestLock"/>
    /// itself before calling into the real host startup. <see langword="true"/> for every ordinary
    /// factory. Overridden to <see langword="false"/> only by factories (e.g.
    /// <see cref="ProductionLikeWebApplicationFactory"/>) that already hold this same lock for their
    /// entire construct-through-dispose lifetime — a second acquisition on the non-reentrant
    /// underlying semaphore, from the same instance, would deadlock itself.
    /// </summary>
    protected virtual bool CreateHostAcquiresEnvironmentVariableLock => true;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["BeeDay:Persistence:SqlServer:ConnectionString"] = ConnectionString,
                ["BeeDay:Email:Development:Enabled"] = "false"
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

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Program.cs's production startup guards (and every ValidateOnStart Options check) read
        // process-wide environment variables synchronously during this call. Serialized against
        // ProductionLikeWebApplicationFactory/ProductionOriginGuardTests's factory, which mutate those
        // same variables around their own lifecycle — see ProductionEnvironmentVariableTestLock.
        // Factories that already hold that lock for their whole lifetime opt out here (see
        // CreateHostAcquiresEnvironmentVariableLock) to avoid deadlocking themselves.
        var host = CreateHostAcquiresEnvironmentVariableLock
            ? CreateHostUnderLock(builder)
            : base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BeeDayDbContext>>();
        using var context = contextFactory.CreateDbContext();
        context.Database.Migrate();

        return host;
    }

    private IHost CreateHostUnderLock(IHostBuilder builder)
    {
        using (ProductionEnvironmentVariableTestLock.Acquire())
        {
            return base.CreateHost(builder);
        }
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
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        // The nickname must be unique per User.CompleteProfile; derive it from the (always
        // distinct, per test) email rather than the shared default name to avoid collisions
        // across the multiple users seeded into one IClassFixture-shared factory. Truncated to
        // Nickname.MaximumLength (20) with a hash suffix when the local-part alone would exceed it.
        var rawNickname = new string(email
            .Split('@')[0]
            .Where(char.IsLetterOrDigit)
            .ToArray())
            .ToLowerInvariant();
        var nickname = rawNickname.Length <= BeeDay.Domain.ValueObjects.Nickname.MaximumLength
            ? rawNickname
            : rawNickname[..14] + Math.Abs(rawNickname.GetHashCode()).ToString(CultureInfo.InvariantCulture).PadLeft(6, '0')[..6];

        var user = User.Create(name, email, passwordService.Hash(password));
        if (confirmEmail)
        {
            user.ConfirmEmail(user.CreatedAtUtc);
        }

        user.CompleteProfile(nickname, avatar: null);
        await repository.AddAsync(user);

        return user;
    }

    public async Task<User?> FindUserAsync(string email)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await repository.GetByEmailAsync(email);
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        await repository.UpdateAsync(userId, user => user.SetActive(false));
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
    /// <remarks>
    /// Returns <see cref="AsyncServiceScope"/> (from <c>CreateAsyncScope()</c>), not the plain
    /// <see cref="IServiceScope"/> <c>CreateScope()</c> gives — cross-Aggregate handlers resolve a
    /// Transient <c>IUnitOfWork</c> (<c>EfUnitOfWork</c>), which implements only
    /// <see cref="IAsyncDisposable"/>. Disposing this scope synchronously (<c>using</c>) throws once
    /// the container discovers that tracked, async-only disposable at scope teardown; the caller must
    /// use <c>await using</c>.
    /// </remarks>
    public AsyncServiceScope CreateAuthenticatedScope(Guid userId, int sessionVersion)
    {
        var scope = Services.CreateAsyncScope();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(BeeDayClaimTypes.SessionVersion, sessionVersion.ToString(CultureInfo.InvariantCulture))
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
        IssueTokenAsync(userId, BeeDay.Domain.Enums.UserTokenType.PasswordReset, expired);

    public Task<string> IssueEmailConfirmationTokenAsync(Guid userId, bool expired = false) =>
        IssueTokenAsync(userId, BeeDay.Domain.Enums.UserTokenType.EmailConfirmation, expired);

    private async Task<string> IssueTokenAsync(Guid userId, BeeDay.Domain.Enums.UserTokenType type, bool expired)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserTokenRepository>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IUserTokenService>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var rawToken = tokenService.GenerateToken();
        var now = clock.UtcNow;
        var createdAt = expired ? now.AddHours(-2) : now;
        var expiresAt = expired ? now.AddHours(-1) : now.AddHours(1);
        await repository.AddAsync(BeeDay.Domain.Entities.UserToken.Create(userId, type, tokenService.HashToken(rawToken), createdAt, expiresAt));

        return rawToken;
    }

    /// <summary>
    /// Produces a validly-encrypted "BeeDay.Auth" cookie value for an arbitrary claim set, using
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

    // WebApplicationFactory<TEntryPoint> implements both IDisposable and IAsyncDisposable — xunit's
    // IClassFixture support prefers the async path when a fixture implements IAsyncDisposable, so
    // DisposeAsync (not Dispose(bool)) is what actually runs at teardown for these tests. Both are
    // overridden so the database is dropped whichever path a caller takes; each is independently safe
    // to call (EnsureDeleted[Async] is a no-op if the database is already gone).
    public override async ValueTask DisposeAsync()
    {
        await DropDatabaseBestEffortAsync();
        await base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DropDatabaseBestEffort();
        }

        base.Dispose(disposing);
    }

    // SQL Server LocalDB refuses to DROP DATABASE while this same process still holds a pooled
    // connection to it (ADO.NET connection pooling keeps the physical connection open even after every
    // DbContext using it is disposed), and a background task still finishing up (BackgroundTaskWorker)
    // can hold one open for a moment after the last test method returns. ClearAllPools() releases idle
    // pooled connections immediately but can't force-close one still actively in use — a handful of
    // retries clears that up reliably, mirroring EmailCaptureWebApplicationFactory's file-cleanup retry
    // loop. This is teardown hygiene only; every attempt is swallowed, but Dispose does not silently
    // give up after the first failure the way it used to.
    private async Task DropDatabaseBestEffortAsync()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                using var scope = Services.CreateScope();
                var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BeeDayDbContext>>();
                await using var context = await contextFactory.CreateDbContextAsync();

                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                await context.Database.EnsureDeletedAsync();
                return;
            }
            catch (Exception)
            {
                await Task.Delay(100);
            }
        }
    }

    private void DropDatabaseBestEffort()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                using var scope = Services.CreateScope();
                var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BeeDayDbContext>>();
                using var context = contextFactory.CreateDbContext();

                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                context.Database.EnsureDeleted();
                return;
            }
            catch (Exception)
            {
                Thread.Sleep(100);
            }
        }
    }
}
