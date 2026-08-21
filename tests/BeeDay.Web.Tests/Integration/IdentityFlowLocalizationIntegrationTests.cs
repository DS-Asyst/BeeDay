using AngleSharp.Html.Parser;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Exercises the remaining public, authentication-adjacent pages migrated onto the Epic 23
/// localization infrastructure: CreateProfile, ForgotPassword, ResetPassword, ConfirmEmail,
/// ResendConfirmation and EmailConfirmationSent. The two flows explicitly called out — Login to
/// Create Account and Login to Forgot Password — are proven end to end (culture set as if from
/// Home, Login rendered, then the destination page reached exactly as the visible link on Login
/// would take a real visitor) in both supported cultures; the remaining pages get representative
/// EN/PT rendering coverage.
/// </summary>
/// <remarks>
/// Assertions run against <see cref="DecodeBodyTextAsync"/>, not the raw HTML string: Blazor's static
/// HTML output writes non-ASCII characters as numeric character references (e.g. "ç" as
/// "&amp;#xE7;"), so a literal "confirmação" never appears byte-for-byte in the response even
/// though the page is correctly rendered in Portuguese. Decoding through AngleSharp is what a
/// real browser (and a real user) sees, so it is what these assertions check.
/// </remarks>
public sealed class IdentityFlowLocalizationIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task LoginToCreateAccount_Portuguese_PreservesCultureAcrossTheFlow()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");

        var loginHtml = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("href=\"/profile/create\"", loginHtml, StringComparison.Ordinal);
        Assert.Contains("Criar conta", await DecodeBodyTextAsync(loginHtml, cancellationToken), StringComparison.Ordinal);

        var createProfileHtml = await client.GetStringAsync("/profile/create", cancellationToken);
        var createProfileText = await DecodeBodyTextAsync(createProfileHtml, cancellationToken);
        Assert.Contains("Crie sua conta", createProfileText, StringComparison.Ordinal);
        Assert.Contains("Comece sua jornada no beeday.", createProfileText, StringComparison.Ordinal);
        Assert.DoesNotContain("Create your account", createProfileText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginToCreateAccount_English_RendersEnglishThroughout()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();

        var loginHtml = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("Create account", loginHtml, StringComparison.Ordinal);

        var createProfileHtml = await client.GetStringAsync("/profile/create", cancellationToken);
        Assert.Contains("Create your account", createProfileHtml, StringComparison.Ordinal);
        Assert.Contains("Start your journey in beeday.", createProfileHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginToForgotPassword_Portuguese_PreservesCultureAcrossTheFlow()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");

        var loginHtml = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("href=\"/account/forgot-password\"", loginHtml, StringComparison.Ordinal);
        Assert.Contains("Esqueceu a senha?", await DecodeBodyTextAsync(loginHtml, cancellationToken), StringComparison.Ordinal);

        var forgotPasswordHtml = await client.GetStringAsync("/account/forgot-password", cancellationToken);
        var forgotPasswordText = await DecodeBodyTextAsync(forgotPasswordHtml, cancellationToken);
        Assert.Contains("Esqueci minha senha", forgotPasswordText, StringComparison.Ordinal);
        Assert.Contains("VOLTAR PARA O LOGIN", forgotPasswordText, StringComparison.Ordinal);
        Assert.DoesNotContain("Forgot password", forgotPasswordText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginToForgotPassword_English_RendersEnglishThroughout()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();

        var loginHtml = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("Forgot password?", loginHtml, StringComparison.Ordinal);

        var forgotPasswordHtml = await client.GetStringAsync("/account/forgot-password", cancellationToken);
        Assert.Contains("Forgot password", forgotPasswordHtml, StringComparison.Ordinal);
        Assert.Contains("BACK TO SIGN IN", forgotPasswordHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResetPassword_RendersPerCulture()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var englishClient = factory.CreateClient();
        using var portugueseClient = factory.CreateClient();
        portugueseClient.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");

        var englishHtml = await englishClient.GetStringAsync("/account/reset-password", cancellationToken);
        var portugueseText = await DecodeBodyTextAsync(
            await portugueseClient.GetStringAsync("/account/reset-password", cancellationToken), cancellationToken);

        Assert.Contains("Reset password", englishHtml, StringComparison.Ordinal);
        Assert.Contains("Choose a new password for your account.", englishHtml, StringComparison.Ordinal);
        Assert.Contains("Redefinir senha", portugueseText, StringComparison.Ordinal);
        Assert.Contains("Escolha uma nova senha para sua conta.", portugueseText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConfirmEmail_WithoutToken_RendersInvalidLinkStatePerCulture()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var englishClient = factory.CreateClient();
        using var portugueseClient = factory.CreateClient();
        portugueseClient.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");

        var englishHtml = await englishClient.GetStringAsync("/account/confirm-email", cancellationToken);
        var portugueseText = await DecodeBodyTextAsync(
            await portugueseClient.GetStringAsync("/account/confirm-email", cancellationToken), cancellationToken);

        Assert.Contains("Invalid confirmation link", englishHtml, StringComparison.Ordinal);
        Assert.Contains("The confirmation token is missing.", englishHtml, StringComparison.Ordinal);
        Assert.Contains("Link de confirmação inválido", portugueseText, StringComparison.Ordinal);
        Assert.Contains("O token de confirmação está ausente.", portugueseText, StringComparison.Ordinal);
    }

    // EPIC 30 Sprint 30.10: no longer expressible as a plain-HttpClient integration test. ConfirmEmail
    // now (correctly — closing a real double-invocation bug: see ConfirmEmail.razor's
    // OnInitializedAsync remarks) only sends ConfirmEmailCommand on the interactive render pass, and
    // client.GetStringAsync only ever observes the static prerender pass, which never reaches that
    // pass. The equivalent localization coverage — including proof the raw domain exception text
    // never leaks — now lives in
    // BeeDay.Web.Tests.Components.Identity.ConfirmEmailTests.InteractivePass_WithExpiredToken_RendersTheLocalizedExpiredMessage_NotTheRawDomainText,
    // a bUnit test that can explicitly set RendererInfo.IsInteractive to reach that pass.

    [Fact]
    public async Task ResendConfirmation_RendersPerCulture()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var englishClient = factory.CreateClient();
        using var portugueseClient = factory.CreateClient();
        portugueseClient.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");

        var englishHtml = await englishClient.GetStringAsync("/account/resend-confirmation", cancellationToken);
        var portugueseText = await DecodeBodyTextAsync(
            await portugueseClient.GetStringAsync("/account/resend-confirmation", cancellationToken), cancellationToken);

        Assert.Contains("Resend confirmation", englishHtml, StringComparison.Ordinal);
        Assert.Contains("Resend Email", englishHtml, StringComparison.Ordinal);
        Assert.Contains("Reenviar confirmação", portugueseText, StringComparison.Ordinal);
        Assert.Contains("Reenviar E-mail", portugueseText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EmailConfirmationSent_RendersPerCulture()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var englishClient = factory.CreateClient();
        using var portugueseClient = factory.CreateClient();
        portugueseClient.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");

        var englishHtml = await englishClient.GetStringAsync("/account/email-confirmation-sent", cancellationToken);
        var portugueseText = await DecodeBodyTextAsync(
            await portugueseClient.GetStringAsync("/account/email-confirmation-sent", cancellationToken), cancellationToken);

        Assert.Contains("Check your email", englishHtml, StringComparison.Ordinal);
        Assert.Contains("your email address", englishHtml, StringComparison.Ordinal);
        Assert.Contains("Verifique seu e-mail", portugueseText, StringComparison.Ordinal);
        Assert.Contains("seu endereço de e-mail", portugueseText, StringComparison.Ordinal);
    }

    private static async Task<string> DecodeBodyTextAsync(string html, CancellationToken cancellationToken)
    {
        var document = await new HtmlParser().ParseDocumentAsync(html, cancellationToken);
        return document.Body?.TextContent ?? string.Empty;
    }
}
