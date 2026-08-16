using System.Net;
using System.Security.Claims;
using AngleSharp.Html.Parser;
using BeeDay.Web.Services.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Sprint 23.6's representative end-to-end proof: an authenticated user with an explicit
/// BeeDay.Culture cookie hitting /wallet gets both the shared MainLayout shell (proven per-culture
/// since Sprint 23.4) AND the Wallet page's own content in that same culture, in one real HTTP
/// round trip. Assertions run against decoded body text/title, not the raw HTML string — see
/// DashboardLocalizationIntegrationTests for why (numeric HTML character references for non-ASCII
/// output).
/// </summary>
public sealed class WalletLocalizationIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task Wallet_WithPortugueseCultureCookie_RendersEmptyWalletInPortuguese()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("wallet-pt@beeday.invalid", "Password123!");

        var html = await GetWithForgedAuthCookieAsync(user.Id, "c=pt-BR|uic=pt-BR", cancellationToken);
        var document = await new HtmlParser().ParseDocumentAsync(html, cancellationToken);
        var bodyText = document.Body?.TextContent ?? string.Empty;

        Assert.Equal("Carteira | beeday", document.Title);
        Assert.Contains("Finanças pessoais", bodyText, StringComparison.Ordinal);
        Assert.Contains("Nova transação", bodyText, StringComparison.Ordinal);
        Assert.Contains("Nenhuma transação encontrada", bodyText, StringComparison.Ordinal);
        Assert.Contains("Nenhuma tag ainda", bodyText, StringComparison.Ordinal);
        Assert.Contains("Saldo atual", bodyText, StringComparison.Ordinal);
        // Shared MainLayout shell, not just the page's own content.
        Assert.Contains("Perfil", bodyText, StringComparison.Ordinal);
        Assert.Contains("Sair do beeday", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Wallet_WithEnglishCultureCookie_RendersEmptyWalletInEnglish()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("wallet-en@beeday.invalid", "Password123!");

        var html = await GetWithForgedAuthCookieAsync(user.Id, "c=en-US|uic=en-US", cancellationToken);

        Assert.Contains("Wallet | beeday", html, StringComparison.Ordinal);
        Assert.Contains("Personal finance", html, StringComparison.Ordinal);
        Assert.Contains("New transaction", html, StringComparison.Ordinal);
        Assert.Contains("No transactions found", html, StringComparison.Ordinal);
        Assert.Contains("No tags yet", html, StringComparison.Ordinal);
        Assert.Contains("Current balance", html, StringComparison.Ordinal);
        Assert.Contains("Profile", html, StringComparison.Ordinal);
        Assert.Contains("Log out of beeday", html, StringComparison.Ordinal);
    }

    private async Task<string> GetWithForgedAuthCookieAsync(Guid userId, string cultureCookie, CancellationToken cancellationToken)
    {
        var authCookie = factory.CreateRawAuthCookie(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(BeeDayClaimTypes.SessionVersion, "1")
        ]);

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/wallet");
        request.Headers.Add("Cookie", $"BeeDay.Auth={authCookie}; BeeDay.Culture={cultureCookie}");

        using var response = await client.SendAsync(request, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
