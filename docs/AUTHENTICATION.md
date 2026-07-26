# Authentication

LevelUp uses ASP.NET Core cookie authentication.

## Responsibilities

- Web owns HTTP endpoints, cookie issuance, sign-out, redirects, and authorization middleware.
- Application validates authentication requests through abstractions and use-case handlers.
- Infrastructure implements password hashing, email delivery, token generation, and request throttling.
- Domain owns user state and identity invariants without depending on ASP.NET Core.

## Implemented Flows

- Account registration
- Sign in
- Sign out
- Email confirmation
- Resend confirmation email
- Forgot password
- Reset password
- Authenticated account updates

## Security Requirements

- Never log plaintext passwords, confirmation tokens, reset tokens, or email API keys.
- Store production secrets outside committed configuration files.
- Keep authentication cookies HTTP-only and configure transport security for production.
- Require antiforgery validation for state-changing browser requests unless an endpoint has a documented alternative protection mechanism.
- Use POST for state-changing operations such as sign-out.
- Validate redirect targets as local URLs.

## Endpoint security

- Login and logout are POST-only operations protected by antiforgery validation.
- Logout is never exposed through a GET endpoint.
- Authentication failures use the same public message for unknown, inactive, unconfirmed, or invalid accounts.
- Return URLs are restricted to local application paths to prevent open redirects.
- Authentication cookies are HttpOnly, use SameSite=Lax, expire after eight hours, and support sliding expiration.
- Persistent sessions are opt-in through **Remember me** and expire after fourteen days.
- CookieSecurePolicy is `Always` outside Development and `SameAsRequest` during local development.
- Authentication logs contain user identifiers only after successful authentication and never contain email addresses, passwords, tokens, or credential hashes.
