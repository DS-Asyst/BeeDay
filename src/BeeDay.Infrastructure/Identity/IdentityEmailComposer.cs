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
    private sealed record EmailContentKeys(string TitleKey, string IntroductionKey, string FooterKey, string ActionLabelKey);

    private static readonly EmailContentKeys ConfirmationKeys = new(
        "ConfirmationTitle", "ConfirmationIntroduction", "ConfirmationFooter", "ConfirmationActionLabel");

    private static readonly EmailContentKeys ResetKeys = new(
        "ResetTitle", "ResetIntroduction", "ResetFooter", "ResetActionLabel");

    // The single, already-culture-resolved shape both renderers below consume. Keeping this as one
    // cohesive value (rather than 5-6 loose positional string parameters, the Sprint 28.2 shape) is
    // the extension seam 28.4 needs for template/visual work without another signature churn across
    // both flows.
    private sealed record EmailContent(
        string Title,
        string Greeting,
        string Introduction,
        string ActionLabel,
        string ActionUrl,
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
            Title: GetString(keys.TitleKey, culture),
            Greeting: string.Format(culture, GetString("Greeting", culture), displayName),
            Introduction: GetString(keys.IntroductionKey, culture),
            ActionLabel: GetString(keys.ActionLabelKey, culture),
            ActionUrl: BuildUrl(path, rawToken),
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

    // #5247F9 is the single officially approved beeday Brand Color (docs/design-system/01-foundations.md
    // §2.2; CLAUDE.md §13) — the only color literal in this template that carries brand meaning. Email
    // clients cannot consume CSS custom properties, so the literal hex is unavoidable here, but it must
    // still track the canonical token's value rather than an independently chosen shade.
    private const string BrandColor = "#5247F9";

    private static string BuildHtmlTemplate(CultureInfo culture, EmailContent content)
    {
        var safeTitle = WebUtility.HtmlEncode(content.Title);
        var safeGreeting = WebUtility.HtmlEncode(content.Greeting);
        var safeIntroduction = WebUtility.HtmlEncode(content.Introduction);
        var safeActionLabel = WebUtility.HtmlEncode(content.ActionLabel);
        var safeActionUrl = WebUtility.HtmlEncode(content.ActionUrl);
        var safeFooter = WebUtility.HtmlEncode(content.Footer);

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
    // against; the raw content is safe to interpolate directly.
    private static string BuildPlainTextTemplate(EmailContent content) =>
        $"""
        {content.Title}

        {content.Greeting}

        {content.Introduction}

        {content.ActionUrl}

        {content.Footer}
        """;
}
