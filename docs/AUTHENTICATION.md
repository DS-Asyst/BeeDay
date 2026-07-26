# Authentication

LevelUp uses ASP.NET Core cookie authentication.

## Ownership

- Web owns HTTP endpoints, cookie issuance, sign-out, redirects, authorization middleware, and antiforgery integration.
- Application owns authentication commands, validation, and contracts.
- Infrastructure implements password hashing, email delivery, token persistence, and throttling.
- Domain owns user state and identity invariants without ASP.NET Core dependencies.

## Implemented flows

- account registration;
- sign in;
- sign out;
- email confirmation and confirmation resend;
- forgot password and reset password;
- authenticated account updates.

## Cookie policy

- cookie name: `LevelUp.Auth`;
- HTTP-only;
- `SameSite=Lax`;
- eight-hour expiration with sliding renewal;
- persistent sessions are opt-in through **Remember me** and expire after fourteen days;
- `CookieSecurePolicy=SameAsRequest` in Development;
- `CookieSecurePolicy=Always` outside Development.

## Endpoint requirements

- Login and logout are POST-only operations protected by antiforgery validation.
- Logout is not exposed through GET.
- Return URLs are restricted to local application paths.
- Authentication failures use a common public response to avoid account enumeration.
- The cookie principal is revalidated against the persisted active user.

## Security rules

- Never log plaintext passwords, confirmation tokens, reset tokens, API keys, or credential hashes.
- Do not commit production secrets.
- Production public URLs must use HTTPS.
- Production `AllowedHosts` must contain explicit hosts and must not use `*`.
- Authentication logs may contain a user identifier only after successful authentication; they must not contain email addresses or secret material.
