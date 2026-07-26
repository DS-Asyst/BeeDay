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
