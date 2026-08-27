using System.Security.Claims;
using BeeDay.Application.Common.Contracts;
using BeeDay.Application.Common.Events;
using BeeDay.Application.DependencyInjection;
using BeeDay.Application.Features.Authentication.Commands;
using BeeDay.Application.Features.Authentication.Requests;
using BeeDay.Domain.Exceptions;
using BeeDay.Infrastructure.DependencyInjection;
using BeeDay.Web.Components;
using BeeDay.Web.Components.Features.Dashboard.State;
using BeeDay.Web.Components.Features.Experience.Feedback;
using BeeDay.Web.Components.Features.ProfileCreation.State;
using BeeDay.Web.Configuration;
using BeeDay.Web.Diagnostics;
using BeeDay.Web.HealthChecks;
using BeeDay.Web.Localization;
using BeeDay.Web.Services;
using BeeDay.Web.Services.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);


var productionHosting = builder.Configuration
    .GetSection(ProductionHostingOptions.SectionName)
    .Get<ProductionHostingOptions>() ?? new ProductionHostingOptions();

if (!builder.Environment.IsDevelopment())
{
    var publicBaseUrl = builder.Configuration["BeeDay:IdentityEmail:PublicBaseUrl"];
    if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var publicUri)
        || publicUri.Scheme != Uri.UriSchemeHttps)
    {
        throw new InvalidOperationException(
            "BeeDay:IdentityEmail:PublicBaseUrl must be an absolute HTTPS URL in production.");
    }

    if (string.IsNullOrWhiteSpace(productionHosting.DataProtectionKeysDirectory)
        || !Path.IsPathRooted(productionHosting.DataProtectionKeysDirectory))
    {
        throw new InvalidOperationException(
            "BeeDay:Hosting:DataProtectionKeysDirectory must be an absolute path in production.");
    }

    var allowedHosts = builder.Configuration["AllowedHosts"];
    if (string.IsNullOrWhiteSpace(allowedHosts) || allowedHosts.Contains('*', StringComparison.Ordinal))
    {
        throw new InvalidOperationException("AllowedHosts must list explicit production hosts.");
    }

    var keysDirectory = Path.GetFullPath(productionHosting.DataProtectionKeysDirectory);
    Directory.CreateDirectory(keysDirectory);

    var dataProtection = builder.Services
        .AddDataProtection()
        .SetApplicationName("BeeDay")
        .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));

    if (OperatingSystem.IsWindows())
    {
        dataProtection.ProtectKeysWithDpapi(protectToLocalMachine: true);
    }

    if (productionHosting.ForwardedHeaders.Enabled)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = productionHosting.ForwardedHeaders.Headers;
            options.ForwardLimit = productionHosting.ForwardedHeaders.ForwardLimit;
            options.RequireHeaderSymmetry = true;

            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            foreach (var proxy in productionHosting.ForwardedHeaders.KnownProxies)
            {
                if (!System.Net.IPAddress.TryParse(proxy, out var address))
                {
                    throw new InvalidOperationException($"Invalid forwarded-header proxy address: '{proxy}'.");
                }

                options.KnownProxies.Add(address);
            }

            foreach (var network in productionHosting.ForwardedHeaders.KnownNetworks)
            {
                var parts = network.Split('/', 2, StringSplitOptions.TrimEntries);
                if (parts.Length != 2
                    || !System.Net.IPAddress.TryParse(parts[0], out var prefix)
                    || !int.TryParse(parts[1], out var prefixLength))
                {
                    throw new InvalidOperationException($"Invalid forwarded-header network: '{network}'.");
                }

                options.KnownIPNetworks.Add(new System.Net.IPNetwork(prefix, prefixLength));
            }
        });
    }
}

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
    options.UseUtcTimestamp = true;
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.TryAdd("correlationId", context.HttpContext.TraceIdentifier);
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "BeeDay.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events.OnRedirectToLogin = context =>
        {
            var returnUrl = Uri.EscapeDataString(context.Request.PathBase + context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"/login?expired=true&returnUrl={returnUrl}");
            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = async context =>
        {
            var value = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(value, out var userId))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var sessionVersionClaim = context.Principal?.FindFirstValue(BeeDayClaimTypes.SessionVersion);
            if (!int.TryParse(sessionVersionClaim, out var sessionVersion))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var repository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var user = await repository.GetByIdAsync(userId, context.HttpContext.RequestAborted);

            if (user is null || !user.IsActive || user.SessionVersion != sessionVersion)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            // Hands the User this handler already loaded off to AuthenticatedAccountCultureProvider
            // (no extra read) via HttpContext.Items, so a session authenticated purely by this
            // persistent "BeeDay.Auth" cookie — one that never repasses through POST /auth/login,
            // the only other place this precedence rule is applied — still gets its saved language
            // applied. That provider only ever consults this when no explicit BeeDay.Culture cookie
            // already exists; see its own remarks for why request-culture resolution, not this
            // event, is where that has to happen.
            context.HttpContext.Items[AuthenticatedAccountCultureProvider.HttpContextItemsKey] = user;
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture(BeeDayCultures.Default)
        .AddSupportedCultures(BeeDayCultures.Supported)
        .AddSupportedUICultures(BeeDayCultures.Supported);

    // Explicit cookie first (always wins), then the authenticated account's saved language for a
    // request that has neither — no query-string or Accept-Language sniffing yet, so an anonymous
    // visitor's browser language never silently overrides the explicit/default culture before a
    // later Sprint deliberately decides to add that provider.
    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider { CookieName = BeeDayCultures.CookieName },
        new AuthenticatedAccountCultureProvider(builder.Environment.IsDevelopment())
    ];
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BeeDay.Application.Common.Security.ICurrentUserContext, HttpCurrentUserContext>();
builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBeeDayApplication();
builder.Services.AddBeeDayInfrastructure(builder.Configuration);
builder.Services.AddScoped<BeeDayWebService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<AuthenticatedUserInitializer>();
builder.Services.AddScoped<AuthenticatedEntryDestinationResolver>();
builder.Services.AddScoped<AuthenticatedCultureSynchronizer>();
builder.Services.AddScoped<DashboardState>();
builder.Services.AddScoped<BeeDayFeedbackStore>();
builder.Services.AddScoped<INotificationHandler<DomainEventNotification>, BeeDayFeedbackEventHandler>();
builder.Services.AddScoped<ProfileCreationState>();

var app = builder.Build();

// Read only after Build(): WebApplicationFactory-based integration tests inject configuration
// overrides that are merged in around Build(), so an eager read of builder.Configuration taken
// earlier in this file would silently see the un-overridden (production-default) values.
var loginRateLimiterOptions = app.Configuration
    .GetSection(LoginRateLimiterOptions.SectionName)
    .Get<LoginRateLimiterOptions>() ?? new LoginRateLimiterOptions();
var loginRateLimiter = LoginRateLimiterFactory.Create(loginRateLimiterOptions);

if (!app.Environment.IsDevelopment() && productionHosting.ForwardedHeaders.Enabled)
{
    app.UseForwardedHeaders();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseExceptionHandler();

// MapRazorComponents only registers an endpoint for each discovered @page route — it does not
// add an implicit catch-all fallback, so a request to a URL matching no @page (a typo, a stale
// external link, a bookmark) terminates routing with a bare, empty 404 before Blazor's own
// Router/NotFoundPage ever runs. Redirecting to the real /not-found page (itself a normal @page
// route) reuses the same styled, localized NotFound component instead of duplicating it.
//
// Deliberately a redirect scoped to 404 only, not UseStatusCodePagesWithReExecute/WithRedirects
// (EXP32-F014, Sprint 32.2):
//
// - Not ReExecute: with ReExecute the browser's address bar keeps the original, unmatched URL
//   while the server renders /not-found's markup for that request. Once the interactive circuit
//   boots, Blazor's own client-side Router independently re-resolves that same (still-unmatched)
//   address-bar URL and, finding no route either, invokes its NotFoundPage fallback a second time
//   — this time with no HttpContext to source a fresh antiforgery token from, so it silently
//   re-renders NotFound's antiforgery-protected Logout form without a token, breaking Logout (400
//   "Malformed request") for any user who lands on a broken link. A redirect changes the address
//   bar to the real /not-found route before the circuit ever boots, so both the server render and
//   the client Router resolve the same matching route through the normal RouteView path — no
//   fallback re-render, no missing antiforgery state.
// - Not the built-in UseStatusCodePagesWithRedirects("/not-found") convenience method either: it
//   applies to the whole 400-599 range (any empty-bodied response), not just 404 — confirmed live
//   by CultureCookieIntegrationTests.SetCulture_WithUnsupportedCulture_IsRejected turning from a
//   real 400 into a 302 once that convenience method was tried. Genuinely-intentional 4xx
//   responses elsewhere (culture rejection, antiforgery validation, rate limiting) must keep their
//   real status code and body untouched — only a true 404 should ever become /not-found.
app.UseStatusCodePages(statusCodeContext =>
{
    if (statusCodeContext.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
    {
        statusCodeContext.HttpContext.Response.Redirect("/not-found");
    }

    return Task.CompletedTask;
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Authentication must run before request-localization here: AuthenticatedAccountCultureProvider
// (registered into RequestLocalizationOptions above) reads the User that OnValidatePrincipal
// stashes on HttpContext.Items, so that hand-off has to have already happened by the time
// UseRequestLocalization() asks its providers to resolve this request's culture. Anonymous
// requests are unaffected either way — UseAuthentication() runs unconditionally but only
// populates HttpContext.Items when a valid "BeeDay.Auth" cookie is actually present.
app.UseAuthentication();
app.UseAuthorization();

app.UseRequestLocalization();

app.UseAntiforgery();
app.MapStaticAssets();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});


app.MapPost("/auth/login", async (
    HttpContext httpContext,
    ISender sender,
    AuthenticatedCultureSynchronizer cultureSynchronizer,
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string? returnUrl,
    [FromForm] bool? rememberMe,
    ILoggerFactory loggerFactory) =>
{
    try
    {
        var user = await sender.Send(new AuthenticateUserCommand(new AuthenticateUserRequest(email, password)));
        // EPIC 30 Sprint 30.22 (BD30-F048): Name/Email were previously also written here, but
        // nothing anywhere in the app ever reads ClaimTypes.Name/Email back (confirmed by repo-wide
        // search, including the implicit ClaimsPrincipal.Identity.Name property, which no code
        // touches either) — they only ever sat as increasingly-stale PII in the up-to-14-day
        // "remember me" cookie. Removed rather than kept in sync on every Name/Email edit, since no
        // code path needs them at all.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(BeeDayClaimTypes.SessionVersion, user.SessionVersion.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authenticationProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe == true,
            AllowRefresh = true
        };
        if (rememberMe == true)
        {
            authenticationProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14);
        }

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authenticationProperties);

        // SignInAsync only affects the outgoing response cookie — HttpContext.User for the rest of
        // THIS request is still whatever it was when the pipeline started (anonymous, since no auth
        // cookie was present yet). Assigning it here is what lets ICurrentUserContext, and therefore
        // the same GetCurrentUserQuery/UpdateCurrentUserPreferencesCommand Settings itself uses,
        // resolve the just-authenticated user for the culture synchronization immediately below.
        httpContext.User = principal;

        // Precedence rule: an explicit BeeDay.Culture cookie already on this request wins (and the
        // account converges to it); otherwise the account's saved language becomes the effective
        // culture. See AuthenticatedCultureSynchronizer for the full rule.
        await cultureSynchronizer.SynchronizeAtLoginAsync(httpContext, httpContext.RequestAborted);

        loggerFactory.CreateLogger("BeeDay.Authentication").LogInformation(
            "Authentication.LoginSucceeded UserId={UserId} RememberMe={RememberMe}", user.Id, rememberMe == true);

        var destination = LoginDestinationResolver.Resolve(
            user.HasProfile,
            user.HasCompletedOnboarding,
            returnUrl);

        return Results.LocalRedirect(destination);
    }
    catch (InvalidDomainStateException)
    {
        loggerFactory.CreateLogger("BeeDay.Authentication").LogWarning(
            "Authentication.LoginFailed TraceId={TraceId} Reason={Reason}",
            httpContext.TraceIdentifier,
            "InvalidCredentials");
        return Results.LocalRedirect("/login?error=invalid");
    }
}).AddEndpointFilter(async (context, next) =>
{
    using var lease = loginRateLimiter.AttemptAcquire(context.HttpContext);
    if (!lease.IsAcquired)
    {
        context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("BeeDay.Authentication")
            .LogWarning("Authentication.LoginRateLimited TraceId={TraceId}", context.HttpContext.TraceIdentifier);

        // Same generic wording regardless of whether the email belongs to a real account.
        return Results.Text("Too many attempts. Please wait and try again.", statusCode: StatusCodes.Status429TooManyRequests);
    }

    return await next(context);
});

app.MapPost("/auth/logout", async (HttpContext httpContext, [FromForm] string? returnUrl, ILoggerFactory loggerFactory) =>
{
    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    loggerFactory.CreateLogger("BeeDay.Authentication").LogInformation(
        "Authentication.LogoutSucceeded UserId={UserId} TraceId={TraceId}",
        userId,
        httpContext.TraceIdentifier);

    return Results.LocalRedirect(LoginDestinationResolver.ResolveLogout(returnUrl));
}).RequireAuthorization();

app.MapPost("/culture/set", (HttpContext httpContext, [FromForm] string culture, [FromForm] string? returnUrl) =>
{
    if (!BeeDayCultures.Supported.Contains(culture, StringComparer.OrdinalIgnoreCase))
    {
        return Results.BadRequest();
    }

    httpContext.Response.Cookies.Append(
        BeeDayCultures.CookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        BeeDayCultures.CreateCookieOptions(app.Environment.IsDevelopment()));

    var destination = LoginDestinationResolver.IsLocalPath(returnUrl) ? returnUrl! : "/";
    return Results.LocalRedirect(destination);
});

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

/// <summary>Exposes the top-level Program for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
