# Authentication

## Scope

LevelUp uses ASP.NET Core cookie authentication. Authentication state is isolated by the authenticated user identifier and validated against the current persisted user on cookie validation.

## Session policy

- Session-only cookie by default.
- `Remember me` creates a persistent cookie valid for 14 days.
- Session cookies use an 8-hour authentication ticket and sliding renewal.
- Cookies are `HttpOnly`, `SameSite=Lax`, and `Secure` when HTTPS is active.
- Expired or rejected sessions return to `/login?expired=true` with a validated local return URL.
- Logout invalidates the authentication cookie.

## Email tokens

- Email confirmation tokens expire after 24 hours.
- Password reset tokens expire after 1 hour.
- Issuing a replacement revokes previous active tokens of the same type.
- Tokens are stored only as hashes and must never be written to logs.

## Abuse prevention

Confirmation resend and password reset requests have a server-side 60-second cooldown per normalized email and operation. The UI also displays the resend countdown, but server enforcement is authoritative.

## User-facing messages

Password recovery always returns the same generic response regardless of account existence. Login also uses a generic invalid-credentials message except when an existing account specifically requires email confirmation.

## Audit events

Authentication uses structured application logs for login success, login refusal, and logout. Logs must not contain passwords, cookies, raw tokens, reset links, or raw email addresses. Failed login email values are represented by a SHA-256 hash.

## Development email testing

When Resend is disabled, messages are captured under the configured `LevelUp:Email:Development:Directory`. The default is `src/LevelUp.Web/Data/Emails` relative to the Web project working directory.

## Verification checklist

1. Register and confirm a new account.
2. Verify duplicate submission is blocked by busy states.
3. Verify confirmation resend is blocked for 60 seconds in both UI and server.
4. Request password reset for existing and unknown addresses and compare responses.
5. Verify expired, invalid, replaced, and already-used token states.
6. Sign in with and without `Remember me` and inspect cookie persistence.
7. Expire or invalidate the session and verify the session-expired login message.
8. Logout and confirm protected pages require authentication.
