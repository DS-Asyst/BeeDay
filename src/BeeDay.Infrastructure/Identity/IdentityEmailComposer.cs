using System.Globalization;
using System.Net;
using System.Resources;
using BeeDay.Application.Common.Identity;
using BeeDay.Domain.Enums;
using BeeDay.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace BeeDay.Infrastructure.Identity;

public sealed class IdentityEmailComposer(IOptions<IdentityEmailOptions> options) : IIdentityEmailComposer
{
    private readonly IdentityEmailOptions _options = options.Value;

    // A narrow, Infrastructure-owned resource catalog for transactional-email strings only (ADR-006) —
    // deliberately not the Web project's IStringLocalizer/resx convention (19 catalogs under
    // src/BeeDay.Web/), which Infrastructure cannot depend on without inverting the dependency
    // direction. ResourceManager.GetString(name, culture) takes an explicit CultureInfo per call, so
    // no thread's CurrentUICulture is ever read or mutated — safe for concurrent requests composing
    // email for different recipients' languages at the same time.
    private static readonly ResourceManager Resources = new(
        "BeeDay.Infrastructure.Identity.EmailResources",
        typeof(IdentityEmailComposer).Assembly);

    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo PortugueseCulture = CultureInfo.GetCultureInfo("pt-BR");

    public EmailMessage ComposeEmailConfirmation(string recipient, string displayName, string rawToken, UserLanguage language)
    {
        var culture = ResolveCulture(language);
        var url = BuildUrl(_options.ConfirmationPath, rawToken);
        var title = GetString("ConfirmationTitle", culture);
        var introduction = GetString("ConfirmationIntroduction", culture);
        var footer = GetString("ConfirmationFooter", culture);
        var actionLabel = GetString("ConfirmationActionLabel", culture);
        var body = BuildHtmlTemplate(culture, title, displayName, introduction, actionLabel, url, footer);
        var plainText = BuildPlainTextTemplate(culture, title, displayName, introduction, url, footer);
        return new EmailMessage(recipient, title, body, plainText);
    }

    public EmailMessage ComposePasswordReset(string recipient, string displayName, string rawToken, UserLanguage language)
    {
        var culture = ResolveCulture(language);
        var url = BuildUrl(_options.PasswordResetPath, rawToken);
        var title = GetString("ResetTitle", culture);
        var introduction = GetString("ResetIntroduction", culture);
        var footer = GetString("ResetFooter", culture);
        var actionLabel = GetString("ResetActionLabel", culture);
        var body = BuildHtmlTemplate(culture, title, displayName, introduction, actionLabel, url, footer);
        var plainText = BuildPlainTextTemplate(culture, title, displayName, introduction, url, footer);
        return new EmailMessage(recipient, title, body, plainText);
    }

    // The only UserLanguage -> culture mapping Infrastructure is allowed to own (ADR-006). Deliberately
    // not a reuse of BeeDay.Web.Localization.BeeDayCultures.FromUserLanguage — Infrastructure cannot
    // reference Web, so this two-line switch is the minimal, unavoidable duplication at the boundary,
    // not a second localization system.
    private static CultureInfo ResolveCulture(UserLanguage language) => language switch
    {
        UserLanguage.Portuguese => PortugueseCulture,
        _ => EnglishCulture
    };

    private static string GetString(string name, CultureInfo culture) =>
        Resources.GetString(name, culture)
            ?? throw new InvalidOperationException($"Missing transactional email resource '{name}'.");

    private string BuildUrl(string path, string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);
        var baseUri = new Uri(EnsureTrailingSlash(_options.PublicBaseUrl), UriKind.Absolute);
        var relativePath = path.TrimStart('/');
        var uriBuilder = new UriBuilder(new Uri(baseUri, relativePath))
        {
            Query = $"token={Uri.EscapeDataString(rawToken)}"
        };
        return uriBuilder.Uri.AbsoluteUri;
    }

    private static string EnsureTrailingSlash(string value) => value.EndsWith('/') ? value : $"{value}/";

    // #5247F9 is the single officially approved beeday Brand Color (docs/design-system/01-foundations.md
    // §2.2; CLAUDE.md §13) — the only color literal in this template that carries brand meaning. Email
    // clients cannot consume CSS custom properties, so the literal hex is unavoidable here, but it must
    // still track the canonical token's value rather than an independently chosen shade.
    private const string BrandColor = "#5247F9";

    private static string BuildHtmlTemplate(
        CultureInfo culture,
        string title,
        string displayName,
        string introduction,
        string actionLabel,
        string actionUrl,
        string footer)
    {
        var greeting = string.Format(culture, GetString("Greeting", culture), displayName);
        var safeTitle = WebUtility.HtmlEncode(title);
        var safeGreeting = WebUtility.HtmlEncode(greeting);
        var safeIntroduction = WebUtility.HtmlEncode(introduction);
        var safeActionLabel = WebUtility.HtmlEncode(actionLabel);
        var safeActionUrl = WebUtility.HtmlEncode(actionUrl);
        var safeFooter = WebUtility.HtmlEncode(footer);

        return $$"""
        <!doctype html>
        <html lang="{{culture.Name}}">
        <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"></head>
        <body style="margin:0;background:#17131f;color:#f4efff;font-family:Arial,sans-serif">
          <div style="max-width:560px;margin:0 auto;padding:40px 24px">
            <h1 style="margin:0 0 24px;color:#ffffff">{{safeTitle}}</h1>
            <p>{{safeGreeting}}</p>
            <p>{{safeIntroduction}}</p>
            <p style="margin:32px 0">
              <a href="{{safeActionUrl}}" style="display:inline-block;background:{{BrandColor}};color:#ffffff;text-decoration:none;padding:14px 22px;border-radius:6px;font-weight:700">{{safeActionLabel}}</a>
            </p>
            <p style="font-size:13px;color:#b8aecb">{{safeFooter}}</p>
          </div>
        </body>
        </html>
        """;
    }

    // Resend (and every mainstream mail provider) accepts a plain-text alternative alongside the HTML
    // body — required by clients that don't render HTML and improves spam-filter scoring for the ones
    // that do. No encoding needed: this is not markup, so there is no injection surface to escape
    // against; the raw display name and URL are safe to interpolate directly.
    private static string BuildPlainTextTemplate(
        CultureInfo culture,
        string title,
        string displayName,
        string introduction,
        string actionUrl,
        string footer)
    {
        var greeting = string.Format(culture, GetString("Greeting", culture), displayName);
        return $"""
        {title}

        {greeting}

        {introduction}

        {actionUrl}

        {footer}
        """;
    }
}
