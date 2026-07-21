using LevelUp.Application.DependencyInjection;
using LevelUp.Infrastructure.DependencyInjection;
using LevelUp.Web.Components;
using LevelUp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLevelUpApplication();
builder.Services.AddLevelUpInfrastructure(builder.Configuration);
builder.Services.AddScoped<LevelUpWebService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseAntiforgery();
app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
