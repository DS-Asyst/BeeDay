using System.Net;
using LevelUp.Application.Common.Identity;
using LevelUp.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace LevelUp.Infrastructure.Identity;

public sealed class IdentityEmailComposer(IOptions<IdentityEmailOptions> options) : IIdentityEmailComposer
{
    private readonly IdentityEmailOptions _options = options.Value;

    public EmailMessage ComposeEmailConfirmation(string recipient, string displayName, string rawToken)
    {
        var url = BuildUrl(_options.ConfirmationPath, rawToken);
        var safeName = WebUtility.HtmlEncode(displayName);
        var safeUrl = WebUtility.HtmlEncode(url);
        var body = BuildTemplate(
            "Confirm your LevelUp email",
            $"Hello, {safeName}!",
            "Confirm your email address to activate your LevelUp account.",
            "Confirm email",
            safeUrl,
            "This link expires in 24 hours and can only be used once.");
        return new EmailMessage(recipient, "Confirm your LevelUp email", body);
    }

    public EmailMessage ComposePasswordReset(string recipient, string displayName, string rawToken)
    {
        var url = BuildUrl(_options.PasswordResetPath, rawToken);
        var safeName = WebUtility.HtmlEncode(displayName);
        var safeUrl = WebUtility.HtmlEncode(url);
        var body = BuildTemplate(
            "Reset your LevelUp password",
            $"Hello, {safeName}!",
            "A password reset was requested for your LevelUp account.",
            "Reset password",
            safeUrl,
            "This link expires in 1 hour and can only be used once. Ignore this email if you did not request it.");
        return new EmailMessage(recipient, "Reset your LevelUp password", body);
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

    private static string BuildTemplate(
        string title,
        string greeting,
        string introduction,
        string actionLabel,
        string actionUrl,
        string footer) => $$"""
        <!doctype html>
        <html lang="en">
        <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"></head>
        <body style="margin:0;background:#17131f;color:#f4efff;font-family:Arial,sans-serif">
          <div style="max-width:560px;margin:0 auto;padding:40px 24px">
            <h1 style="margin:0 0 24px;color:#ffffff">{{WebUtility.HtmlEncode(title)}}</h1>
            <p>{{greeting}}</p>
            <p>{{WebUtility.HtmlEncode(introduction)}}</p>
            <p style="margin:32px 0">
              <a href="{{actionUrl}}" style="display:inline-block;background:#7A4FCB;color:#ffffff;text-decoration:none;padding:14px 22px;border-radius:6px;font-weight:700">{{WebUtility.HtmlEncode(actionLabel)}}</a>
            </p>
            <p style="font-size:13px;color:#b8aecb">{{WebUtility.HtmlEncode(footer)}}</p>
          </div>
        </body>
        </html>
        """;
}
