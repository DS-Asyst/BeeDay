using System.Net;
using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace BeeDay.Infrastructure.Identity;

public sealed class IdentityEmailComposer(IOptions<IdentityEmailOptions> options) : IIdentityEmailComposer
{
    private readonly IdentityEmailOptions _options = options.Value;

    public EmailMessage ComposeEmailConfirmation(string recipient, string displayName, string rawToken)
    {
        var url = BuildUrl(_options.ConfirmationPath, rawToken);
        const string title = "Confirm your beeday email";
        const string introduction = "Confirm your email address to activate your beeday account.";
        const string footer = "This link expires in 24 hours and can only be used once.";
        var body = BuildHtmlTemplate(title, displayName, introduction, "Confirm email", url, footer);
        var plainText = BuildPlainTextTemplate(title, displayName, introduction, url, footer);
        return new EmailMessage(recipient, title, body, plainText);
    }

    public EmailMessage ComposePasswordReset(string recipient, string displayName, string rawToken)
    {
        var url = BuildUrl(_options.PasswordResetPath, rawToken);
        const string title = "Reset your beeday password";
        const string introduction = "A password reset was requested for your beeday account.";
        const string footer = "This link expires in 1 hour and can only be used once. Ignore this email if you did not request it.";
        var body = BuildHtmlTemplate(title, displayName, introduction, "Reset password", url, footer);
        var plainText = BuildPlainTextTemplate(title, displayName, introduction, url, footer);
        return new EmailMessage(recipient, title, body, plainText);
    }

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
        string title,
        string displayName,
        string introduction,
        string actionLabel,
        string actionUrl,
        string footer)
    {
        var safeTitle = WebUtility.HtmlEncode(title);
        var safeGreeting = WebUtility.HtmlEncode($"Hello, {displayName}!");
        var safeIntroduction = WebUtility.HtmlEncode(introduction);
        var safeActionLabel = WebUtility.HtmlEncode(actionLabel);
        var safeActionUrl = WebUtility.HtmlEncode(actionUrl);
        var safeFooter = WebUtility.HtmlEncode(footer);

        return $$"""
        <!doctype html>
        <html lang="en">
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
        string title,
        string displayName,
        string introduction,
        string actionUrl,
        string footer) =>
        $"""
        {title}

        Hello, {displayName}!

        {introduction}

        {actionUrl}

        {footer}
        """;
}
