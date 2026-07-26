using System.Security.Claims;
using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Events;
using LevelUp.Application.DependencyInjection;
using LevelUp.Application.Features.Authentication.Commands;
using LevelUp.Application.Features.Authentication.Requests;
using LevelUp.Domain.Exceptions;
using LevelUp.Infrastructure.DependencyInjection;
using LevelUp.Web.Components;
using LevelUp.Web.Components.Features.Character.Feedback;
using LevelUp.Web.Components.Features.CharacterCreation.State;
using LevelUp.Web.Components.Features.Dashboard.State;
using LevelUp.Web.Configuration;
using LevelUp.Web.Diagnostics;
using LevelUp.Web.HealthChecks;
using LevelUp.Web.Services;
using LevelUp.Web.Services.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);


var productionHosting = builder.Configuration
    .GetSection(ProductionHostingOptions.SectionName)
    .Get<ProductionHostingOptions>() ?? new ProductionHostingOptions();

if (!builder.Environment.IsDevelopment())
{
    var publicBaseUrl = builder.Configuration["LevelUp:IdentityEmail:PublicBaseUrl"];
    if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var publicUri)
        || publicUri.Scheme != Uri.UriSchemeHttps)
    {
        throw new InvalidOperationException(
            "LevelUp:IdentityEmail:PublicBaseUrl must be an absolute HTTPS URL in production.");
    }

    var storageDirectory = builder.Configuration["LevelUp:Storage:Directory"];
    if (string.IsNullOrWhiteSpace(storageDirectory) || !Path.IsPathRooted(storageDirectory))
    {
        throw new InvalidOperationException(
            "LevelUp:Storage:Directory must be an absolute path outside the publish directory in production.");
    }

    if (string.IsNullOrWhiteSpace(productionHosting.DataProtectionKeysDirectory)
        || !Path.IsPathRooted(productionHosting.DataProtectionKeysDirectory))
    {
        throw new InvalidOperationException(
            "LevelUp:Hosting:DataProtectionKeysDirectory must be an absolute path in production.");
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
        .SetApplicationName("LevelUp")
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
        options.Cookie.Name = "LevelUp.Auth";
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

            var repository = context.HttpContext.RequestServices.GetRequiredService<ILevelUpRepository>();
            var data = await repository.LoadAsync(context.HttpContext.RequestAborted);
            var user = data.Users.FirstOrDefault(candidate => candidate.Id == userId);

            if (user is null || !user.IsActive)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LevelUp.Application.Common.Security.ICurrentUserContext, HttpCurrentUserContext>();
builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLevelUpApplication();
builder.Services.AddLevelUpInfrastructure(builder.Configuration);
builder.Services.AddScoped<LevelUpWebService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<AuthenticatedUserInitializer>();
builder.Services.AddScoped<DashboardState>();
builder.Services.AddScoped<LevelUpFeedbackStore>();
builder.Services.AddScoped<INotificationHandler<DomainEventNotification>, LevelUpFeedbackEventHandler>();
builder.Services.AddScoped<CharacterCreationState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment() && productionHosting.ForwardedHeaders.Enabled)
{
    app.UseForwardedHeaders();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
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
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string? returnUrl,
    [FromForm] bool? rememberMe,
    ILoggerFactory loggerFactory) =>
{
    try
    {
        var user = await sender.Send(new AuthenticateUserCommand(new AuthenticateUserRequest(email, password)));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
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

        loggerFactory.CreateLogger("LevelUp.Authentication").LogInformation(
            "Authentication.LoginSucceeded UserId={UserId} RememberMe={RememberMe}", user.Id, rememberMe == true);

        var destination = LoginDestinationResolver.Resolve(
            user.HasCharacter,
            user.HasCompletedOnboarding,
            returnUrl);

        return Results.LocalRedirect(destination);
    }
    catch (InvalidDomainStateException)
    {
        loggerFactory.CreateLogger("LevelUp.Authentication").LogWarning(
            "Authentication.LoginFailed TraceId={TraceId} Reason={Reason}",
            httpContext.TraceIdentifier,
            "InvalidCredentials");
        return Results.LocalRedirect("/login?error=invalid");
    }
});

app.MapPost("/auth/logout", async (HttpContext httpContext, [FromForm] string? returnUrl, ILoggerFactory loggerFactory) =>
{
    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    loggerFactory.CreateLogger("LevelUp.Authentication").LogInformation(
        "Authentication.LogoutSucceeded UserId={UserId} TraceId={TraceId}",
        userId,
        httpContext.TraceIdentifier);

    return Results.LocalRedirect(LoginDestinationResolver.ResolveLogout(returnUrl));
}).RequireAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
