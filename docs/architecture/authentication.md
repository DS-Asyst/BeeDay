# Authentication and Identity Architecture

LevelUp uses ASP.NET Core cookie authentication with Application use cases and Infrastructure identity services.

## Implemented flows

- account registration;
- sign in and POST-only sign out;
- email confirmation and resend;
- forgot-password and reset-password flows;
- authenticated account and preference updates;
- persisted user-token handling.

## Ownership

- Domain owns user state, tokens, and identity invariants without ASP.NET Core dependencies.
- Application owns requests, validation, commands, handlers, and service contracts.
- Infrastructure implements PBKDF2 password hashing, secure token services, email composition/delivery, throttling, and clocks.
- Web owns endpoints, cookie creation, antiforgery, return URLs, authorization middleware, and redirects.

## Cookie policy

The configured cookie:

- is named `LevelUp.Auth`;
- is HTTP-only;
- uses `SameSite=Lax`;
- uses an eight-hour sliding session by default;
- supports an explicit persistent fourteen-day session through Remember Me;
- uses `SameAsRequest` in Development and `Always` outside Development;
- revalidates the principal against the persisted active user.

## Security rules

- Never log passwords, password hashes, reset tokens, confirmation tokens, API keys, or secret values.
- Authentication state-changing endpoints require POST and antiforgery protection.
- Return URLs must resolve to local application paths.
- Production requires HTTPS and explicit allowed hosts.
- Secrets belong in environment variables, user secrets, or the deployment platform secret store.
