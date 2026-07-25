using System.Security.Claims;
using LevelUp.Application.Common.Contracts;
using LevelUp.Application.DependencyInjection;
using LevelUp.Application.Features.Authentication.Commands;
using LevelUp.Application.Features.Authentication.Requests;
using LevelUp.Infrastructure.DependencyInjection;
using LevelUp.Web.Components;
using LevelUp.Web.Components.Features.CharacterCreation.State;
using LevelUp.Web.Components.Features.Dashboard.State;
using LevelUp.Web.Diagnostics;
using LevelUp.Web.HealthChecks;
using LevelUp.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

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
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
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
builder.Services.AddScoped<CharacterCreationState>();

var app = builder.Build();

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
    [FromForm] string? returnUrl) =>
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

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            });

        var defaultDestination = !user.HasCharacter
            ? "/character/create"
            : user.HasCompletedOnboarding ? "/daily" : "/onboarding/tutorial";

        var destination = !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//")
            ? returnUrl
            : defaultDestination;

        return Results.LocalRedirect(destination);
    }
    catch
    {
        var encodedEmail = Uri.EscapeDataString(email ?? string.Empty);
        return Results.LocalRedirect($"/login?error=invalid&email={encodedEmail}");
    }
}).DisableAntiforgery();

app.MapGet("/auth/logout", async (HttpContext httpContext, [FromQuery] string? returnUrl) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    var destination = !string.IsNullOrWhiteSpace(returnUrl) &&
                      returnUrl.StartsWith('/') &&
                      !returnUrl.StartsWith("//")
        ? returnUrl
        : "/login";
    return Results.LocalRedirect(destination);
});

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
