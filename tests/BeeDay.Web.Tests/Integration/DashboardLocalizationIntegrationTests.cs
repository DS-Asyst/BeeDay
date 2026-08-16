using System.Net;
using System.Security.Claims;
using AngleSharp.Html.Parser;
using BeeDay.Web.Services.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Sprint 23.5's representative end-to-end proof: an authenticated user with an explicit
/// BeeDay.Culture cookie hitting a Dashboard route gets both the shared MainLayout shell (already
/// covered per-culture by AuthenticatedCultureIntegrationTests since Sprint 23.4) AND the
/// Dashboard page's own content in that same culture, in a single real HTTP round trip.
/// </summary>
/// <remarks>
/// Assertions run against decoded body text/title, not the raw HTML string: Blazor's static HTML
/// output writes non-ASCII characters as numeric character references (e.g. "á" as "&amp;#xE1;"),
/// so a literal "Diário" never appears byte-for-byte in the response even though the page is
/// correctly rendered in Portuguese — see IdentityFlowLocalizationIntegrationTests for the same
/// pattern established in Sprint 23.2.
/// </remarks>
public sealed class DashboardLocalizationIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task Daily_WithPortugueseCultureCookie_RendersEmptyDashboardInPortuguese()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("dashboard-daily-pt@beeday.invalid", "Password123!");

        var html = await GetWithForgedAuthCookieAsync(user.Id, "/daily", "c=pt-BR|uic=pt-BR", cancellationToken);
        var document = await new HtmlParser().ParseDocumentAsync(html, cancellationToken);
        var bodyText = document.Body?.TextContent ?? string.Empty;

        Assert.Equal("Diário | BeeDay", document.Title);
        Assert.Contains("Buscar atividades", html, StringComparison.Ordinal);
        Assert.Contains("Hábitos", bodyText, StringComparison.Ordinal);
        Assert.Contains("Nenhum hábito ainda", bodyText, StringComparison.Ordinal);
        Assert.Contains("Tarefas", bodyText, StringComparison.Ordinal);
        Assert.Contains("Pendências", bodyText, StringComparison.Ordinal);
        Assert.Contains("Projetos", bodyText, StringComparison.Ordinal);
        // Shared MainLayout shell, not just the page's own content.
        Assert.Contains("Perfil", bodyText, StringComparison.Ordinal);
        Assert.Contains("Sair do BeeDay", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Daily_WithEnglishCultureCookie_RendersEmptyDashboardInEnglish()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("dashboard-daily-en@beeday.invalid", "Password123!");

        var html = await GetWithForgedAuthCookieAsync(user.Id, "/daily", "c=en-US|uic=en-US", cancellationToken);

        Assert.Contains("Daily | BeeDay", html, StringComparison.Ordinal);
        Assert.Contains("Search activities", html, StringComparison.Ordinal);
        Assert.Contains("Habits", html, StringComparison.Ordinal);
        Assert.Contains("No habits yet", html, StringComparison.Ordinal);
        Assert.Contains("Tasks", html, StringComparison.Ordinal);
        Assert.Contains("To-Dos", html, StringComparison.Ordinal);
        Assert.Contains("Projects", html, StringComparison.Ordinal);
        Assert.Contains("Profile", html, StringComparison.Ordinal);
        Assert.Contains("Log out of BeeDay", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Profile_WithPortugueseCultureCookie_RendersProfileHomeInPortuguese()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("dashboard-profile-pt@beeday.invalid", "Password123!");

        var html = await GetWithForgedAuthCookieAsync(user.Id, "/profile", "c=pt-BR|uic=pt-BR", cancellationToken);
        var document = await new HtmlParser().ParseDocumentAsync(html, cancellationToken);
        var bodyText = document.Body?.TextContent ?? string.Empty;

        Assert.Equal("Perfil | BeeDay", document.Title);
        Assert.Contains("Escolha um próximo passo e continue seu dia em movimento.", bodyText, StringComparison.Ordinal);
        Assert.Contains("Abrir Diário", bodyText, StringComparison.Ordinal);
        Assert.Contains("Atividade semanal", bodyText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Profile_WithEnglishCultureCookie_RendersProfileHomeInEnglish()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("dashboard-profile-en@beeday.invalid", "Password123!");

        var html = await GetWithForgedAuthCookieAsync(user.Id, "/profile", "c=en-US|uic=en-US", cancellationToken);

        Assert.Contains("Profile | BeeDay", html, StringComparison.Ordinal);
        Assert.Contains("Choose one next step and keep your day moving.", html, StringComparison.Ordinal);
        Assert.Contains("Open Daily", html, StringComparison.Ordinal);
        Assert.Contains("Weekly activity", html, StringComparison.Ordinal);
    }

    private async Task<string> GetWithForgedAuthCookieAsync(Guid userId, string path, string cultureCookie, CancellationToken cancellationToken)
    {
        var authCookie = factory.CreateRawAuthCookie(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(BeeDayClaimTypes.SessionVersion, "1")
        ]);

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("Cookie", $"BeeDay.Auth={authCookie}; BeeDay.Culture={cultureCookie}");

        using var response = await client.SendAsync(request, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
