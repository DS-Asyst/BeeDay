using BeeDay.Web.Components.Features.Authentication;
using BeeDay.Web.Components.Features.Authentication.Pages;
using BeeDay.Web.Tests.Localization;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Components.Authentication;

public sealed class LoginTests
{
    [Fact]
    public void RendersFullscreenNavigationActionsWithCorrectDestinations()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Login>());

        var close = cut.Find("a.public-auth-actions__close");
        Assert.Equal("/", close.GetAttribute("href"));
        Assert.Equal("Close login and return to Home", close.GetAttribute("aria-label"));

        var create = cut.Find("a.public-auth-actions__destination");
        Assert.Equal("/profile/create", create.GetAttribute("href"));
        Assert.Contains("Create account", create.TextContent, StringComparison.Ordinal);
        Assert.Contains("beeday-button--important-white", create.ClassList);

        var forgotPassword = cut.Find("a.beeday-link");
        Assert.Equal("/account/forgot-password", forgotPassword.GetAttribute("href"));
        Assert.Contains("Forgot password?", forgotPassword.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void PreservesAuthenticationContractAndRemovesCardContainer()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Login>());

        Assert.Empty(cut.FindAll(".auth-card"));
        Assert.Equal("/auth/login", cut.Find("form[method='post']").GetAttribute("action"));
        var email = cut.Find("#login-email.beeday-field__control");
        var password = cut.Find("#login-password.beeday-field__control");
        var rememberMe = cut.Find("#login-remember.beeday-checkbox__control[name='rememberMe']");

        Assert.Equal("login-email", cut.Find("label[for='login-email']").GetAttribute("for"));
        Assert.Equal("email", email.GetAttribute("autocomplete"));
        Assert.Equal("current-password", password.GetAttribute("autocomplete"));
        Assert.NotNull(rememberMe);
        Assert.NotNull(cut.Find(".auth-remember .beeday-checkbox__visual"));
        Assert.NotNull(cut.Find("input[name='returnUrl']"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersThePortugueseAuthenticationResources()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<Login>());

        Assert.Contains("Bem-vindo de volta", cut.Find("h1").TextContent, StringComparison.Ordinal);
        Assert.Contains("Entrar", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Esqueceu a senha?", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Criar conta", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Welcome back", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Forgot password?", cut.Markup, StringComparison.Ordinal);
    }

    // EPIC 32 Sprint 32.5 (EXP32-F007): Login posts through a plain HTML form (not EditForm), so
    // an empty/invalid field falls back to the browser's native validation bubble, which showed
    // the browser/OS language instead of beeday's selected culture. Proves the bubble's message
    // now comes from the localized DesignSystemResources catalog (matching every other form's
    // validation copy) rather than the browser default, for both required fields and email format.
    [Fact]
    public void EmailAndPasswordFields_HaveLocalizedNativeValidationMessages()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Login>());

        var email = cut.Find("#login-email");
        var password = cut.Find("#login-password");

        Assert.Contains("Email is required.", email.GetAttribute("oninvalid"), StringComparison.Ordinal);
        Assert.Contains("Enter a valid email address.", email.GetAttribute("oninvalid"), StringComparison.Ordinal);
        Assert.Contains("Password is required.", password.GetAttribute("oninvalid"), StringComparison.Ordinal);
        Assert.Equal("this.setCustomValidity('')", email.GetAttribute("oninput"));
        Assert.Equal("this.setCustomValidity('')", password.GetAttribute("oninput"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_NativeValidationMessagesAreLocalized()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<Login>());

        var email = cut.Find("#login-email");
        var password = cut.Find("#login-password");

        Assert.Contains("O e-mail é obrigatório.", email.GetAttribute("oninvalid"), StringComparison.Ordinal);
        Assert.Contains("Informe um endereço de e-mail válido.", email.GetAttribute("oninvalid"), StringComparison.Ordinal);
        Assert.Contains("A senha é obrigatória.", password.GetAttribute("oninvalid"), StringComparison.Ordinal);
    }

    [Fact]
    public void SigningInResource_WithApostrophesAndBackslashes_IsSafelyEscapedInTheInlineHandler()
    {
        using var context = new BunitContext().WithLocalization();
        const string adversarial = "Isn't a \\test\\ 'string'?";
        context.Services.AddSingleton<IStringLocalizer<AuthenticationResources>>(
            new SingleKeyOverrideLocalizer<AuthenticationResources>("SigningIn", adversarial));

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Login>());

        var onsubmit = cut.Find("form.auth-form").GetAttribute("onsubmit")!;

        // The same two-step escape the page applies before interpolating into the single-quoted
        // JS string literal — recomputed here rather than exposing the page's private helper.
        var expectedEscaped = adversarial
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal);

        Assert.Contains($"textContent='{expectedEscaped}';", onsubmit, StringComparison.Ordinal);

        // No unescaped apostrophe survives to prematurely terminate the JS string literal.
        Assert.DoesNotContain("textContent='Isn'", onsubmit, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthenticationResource_WithHtmlSpecialCharacters_IsAttributeEncodedNotInjected()
    {
        using var context = new BunitContext().WithLocalization();
        const string adversarial = "Close & \"quit\" <b>now</b>";
        context.Services.AddSingleton<IStringLocalizer<AuthenticationResources>>(
            new SingleKeyOverrideLocalizer<AuthenticationResources>("CloseAndReturnToHome", adversarial));

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Login>());

        // AngleSharp hands back the decoded attribute value — round-tripping to exactly the
        // adversarial input (quotes and all) proves the '"'/'&' were HTML-encoded on the way out;
        // an un-encoded '"' would instead have terminated the aria-label attribute early,
        // corrupted the surrounding markup, and made this element unfindable by that attribute.
        var close = cut.Find("a.public-auth-actions__close");
        Assert.Equal(adversarial, close.GetAttribute("aria-label"));
        Assert.Contains("&quot;", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("&amp;", cut.Markup, StringComparison.Ordinal);
    }
}
