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

    // EPIC 28, Sprint 28.3: the resource-key set that distinguishes "which flow" from the otherwise
    // identical composition steps both public methods used to repeat inline (Sprint 28.2 shape).
    // Not a generic template framework — just the one seam this Sprint's audit found was real
    // duplication between the two flows.
    private sealed record EmailContentKeys(
        string PreheaderKey,
        string TitleKey,
        string IntroductionKey,
        string FooterKey,
        string ActionLabelKey);

    private static readonly EmailContentKeys ConfirmationKeys = new(
        "ConfirmationPreheader", "ConfirmationTitle", "ConfirmationIntroduction", "ConfirmationFooter", "ConfirmationActionLabel");

    private static readonly EmailContentKeys ResetKeys = new(
        "ResetPreheader", "ResetTitle", "ResetIntroduction", "ResetFooter", "ResetActionLabel");

    // The single, already-culture-resolved shape both renderers below consume. Keeping this as one
    // cohesive value (rather than loose positional string parameters) is the extension seam 28.4's
    // visual work uses to add the preheader/fallback-link fields without another signature churn
    // across both flows.
    private sealed record EmailContent(
        string Preheader,
        string Title,
        string Greeting,
        string Introduction,
        string ActionLabel,
        string ActionUrl,
        string FallbackLinkIntro,
        string Footer);

    public EmailMessage ComposeEmailConfirmation(string recipient, string displayName, string rawToken, UserLanguage language) =>
        Compose(recipient, displayName, rawToken, language, _options.ConfirmationPath, ConfirmationKeys);

    public EmailMessage ComposePasswordReset(string recipient, string displayName, string rawToken, UserLanguage language) =>
        Compose(recipient, displayName, rawToken, language, _options.PasswordResetPath, ResetKeys);

    private EmailMessage Compose(
        string recipient,
        string displayName,
        string rawToken,
        UserLanguage language,
        string path,
        EmailContentKeys keys)
    {
        var culture = ResolveCulture(language);
        var content = new EmailContent(
            Preheader: GetString(keys.PreheaderKey, culture),
            Title: GetString(keys.TitleKey, culture),
            Greeting: string.Format(culture, GetString("Greeting", culture), displayName),
            Introduction: GetString(keys.IntroductionKey, culture),
            ActionLabel: GetString(keys.ActionLabelKey, culture),
            ActionUrl: BuildUrl(path, rawToken),
            FallbackLinkIntro: GetString("FallbackLinkIntro", culture),
            Footer: GetString(keys.FooterKey, culture));

        var body = BuildHtmlTemplate(culture, content);
        var plainText = BuildPlainTextTemplate(content);
        return new EmailMessage(recipient, content.Title, body, plainText);
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

    // Every literal below is a beeday Experience System token value, not an independently chosen
    // shade — email clients cannot consume CSS custom properties, so the hex literals are unavoidable,
    // but each one must keep tracking its source token's value (docs/design-system/01-foundations.md
    // §2, verified 2026-08-16). #5247F9 is the single officially approved beeday Brand Color
    // (CLAUDE.md §13).
    private const string BrandColor = "#5247F9"; // --beeday-color-brand-primary
    private const string CanvasColor = "#F7F7F7"; // --beeday-color-surface-muted
    private const string SurfaceColor = "#FFFFFF"; // --beeday-color-surface / --beeday-color-background
    private const string TextPrimaryColor = "#2F2737"; // --beeday-color-text-primary
    // --beeday-color-text-secondary — used for every "muted" text role in this template (fallback
    // link intro, footer, footer credit line) instead of --beeday-color-text-muted (#817789): EPIC 28,
    // Sprint 28.9's automated axe accessibility scan measured #817789 on white at ~4.26:1, under
    // WCAG AA's 4.5:1 for normal text (the same pre-existing gap already known and deferred for that
    // token elsewhere in the product, DEFER 25.15) — a brand-new template has no reason to inherit
    // that deferred debt. #514858 on white measures ~8.7:1, comfortably AA/AAA-compliant, and is
    // already an approved brand token, not a new color.
    private const string TextSecondaryColor = "#514858";
    private const string BorderColor = "#E5E5E5"; // --beeday-color-border

    // Email clients are not browsers: no CSS custom properties, no guaranteed remote font, and
    // Outlook's desktop renderer (Word engine) needs table-based layout, inline styles, and role=
    // "presentation" to lay out predictably. Priorities, in order: (1) legible and fully usable with
    // every custom font blocked — Nunito/Coiny are progressive enhancement, never a requirement to
    // read or act; (2) Coiny appears only in the brand wordmark (Brand/Display), never in body/CTA
    // text (Product/UI stays Nunito, per docs/design-system/01-foundations.md §3); (3) works with
    // images off (there are none — no remote image is used anywhere in this template); (4) a visible
    // fallback link for clients/policies that strip or don't render the button.
    private static string BuildHtmlTemplate(CultureInfo culture, EmailContent content)
    {
        var safePreheader = WebUtility.HtmlEncode(content.Preheader);
        var safeTitle = WebUtility.HtmlEncode(content.Title);
        var safeGreeting = WebUtility.HtmlEncode(content.Greeting);
        var safeIntroduction = WebUtility.HtmlEncode(content.Introduction);
        var safeActionLabel = WebUtility.HtmlEncode(content.ActionLabel);
        var safeActionUrl = WebUtility.HtmlEncode(content.ActionUrl);
        var safeFallbackLinkIntro = WebUtility.HtmlEncode(content.FallbackLinkIntro);
        var safeFooter = WebUtility.HtmlEncode(content.Footer);
        var productFontStack = "'Nunito','Segoe UI',Arial,sans-serif";
        var brandFontStack = "'Coiny','Nunito','Segoe UI',sans-serif";

        return $$"""
        <!doctype html>
        <html lang="{{culture.Name}}">
        <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <title>{{safeTitle}}</title>
        </head>
        <body style="margin:0;padding:0;background:{{CanvasColor}};">
        <div style="display:none;font-size:1px;line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;mso-hide:all;">{{safePreheader}}</div>
        <div style="display:none;font-size:1px;line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;mso-hide:all;">&#8203;&#847; &#8203;&#847; &#8203;&#847; &#8203;&#847;</div>
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:{{CanvasColor}};">
        <tr>
        <td align="center" style="padding:32px 16px;">
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;background:{{SurfaceColor}};border-radius:12px;">
        <tr>
        <td align="center" style="padding:32px 32px 8px;">
        <span style="font-family:{{brandFontStack}};font-size:28px;line-height:1;color:{{BrandColor}};">beeday</span>
        </td>
        </tr>
        <tr>
        <td style="padding:16px 32px 0;font-family:{{productFontStack}};">
        <h1 style="margin:0 0 20px;font-size:20px;line-height:1.3;font-weight:700;color:{{TextPrimaryColor}};">{{safeTitle}}</h1>
        <p style="margin:0 0 16px;font-size:15px;line-height:1.5;color:{{TextPrimaryColor}};">{{safeGreeting}}</p>
        <p style="margin:0 0 28px;font-size:15px;line-height:1.5;color:{{TextSecondaryColor}};">{{safeIntroduction}}</p>
        <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 0 28px;">
        <tr>
        <td style="border-radius:8px;background:{{BrandColor}};">
        <a href="{{safeActionUrl}}" style="display:inline-block;padding:14px 24px;font-family:{{productFontStack}};font-size:15px;font-weight:700;color:#FFFFFF;text-decoration:none;">{{safeActionLabel}}</a>
        </td>
        </tr>
        </table>
        <p style="margin:0 0 6px;font-size:13px;line-height:1.5;color:{{TextSecondaryColor}};">{{safeFallbackLinkIntro}}</p>
        <p style="margin:0 0 28px;font-size:13px;line-height:1.5;word-break:break-all;"><a href="{{safeActionUrl}}" style="color:{{BrandColor}};">{{safeActionUrl}}</a></p>
        <p style="margin:0 0 24px;font-size:13px;line-height:1.5;color:{{TextSecondaryColor}};">{{safeFooter}}</p>
        </td>
        </tr>
        <tr>
        <td style="padding:16px 32px 32px;border-top:1px solid {{BorderColor}};font-family:{{productFontStack}};">
        <p style="margin:0;font-size:12px;line-height:1.5;color:{{TextSecondaryColor}};">beeday</p>
        </td>
        </tr>
        </table>
        </td>
        </tr>
        </table>
        </body>
        </html>
        """;
    }

    // Resend (and every mainstream mail provider) accepts a plain-text alternative alongside the HTML
    // body — required by clients that don't render HTML and improves spam-filter scoring for the ones
    // that do. No encoding needed: this is not markup, so there is no injection surface to escape
    // against; the raw content is safe to interpolate directly.
    private static string BuildPlainTextTemplate(EmailContent content) =>
        $"""
        beeday

        {content.Title}

        {content.Greeting}

        {content.Introduction}

        {content.ActionLabel}: {content.ActionUrl}

        {content.FallbackLinkIntro}
        {content.ActionUrl}

        {content.Footer}
        """;
}
