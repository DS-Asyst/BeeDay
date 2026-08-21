namespace BeeDay.Web.Diagnostics;

/// <summary>
/// Sets the security-relevant response headers <c>docs/security/01-security-baseline.md</c> §4
/// documents as absent: X-Content-Type-Options, Referrer-Policy, Permissions-Policy. Deliberately
/// does NOT set X-Frame-Options or Content-Security-Policy — ASP.NET Core's Razor Components
/// framework already sends <c>X-Frame-Options: SAMEORIGIN</c> and
/// <c>Content-Security-Policy: frame-ancestors 'self'</c> on every interactive-server-rendered
/// response independently of any app configuration (confirmed by
/// <c>SecurityHeadersIntegrationTests</c>); adding a competing value here would either be silently
/// overwritten by the framework's later write or produce a duplicate/conflicting header depending on
/// response type, and a fuller CSP (script-src etc.) needs its own dedicated, carefully-verified
/// Sprint rather than a header-only addition — see EPIC 30 Sprint 30.22's Audit Ledger entry.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        await next(context);
    }
}
