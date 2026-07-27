# Users and Identity

A User owns account identity, preferences, active state, character association, and identity tokens.

## Invariants

- Email addresses and user names use validated value objects.
- Password material is not stored as plaintext.
- Identity tokens have an explicit type and lifecycle.
- Inactive or missing users cannot retain a valid authenticated principal.
- User-scoped operations must enforce multi-user isolation through the current-user context and ownership checks.

## Preferences

The current domain supports language and theme preferences through `UserLanguage` and `UserTheme`.

## Identity operations

Application use cases coordinate registration, authentication, confirmation, reset, account updates, preference changes, and password changes. Domain state changes remain independent from HTTP and provider details.
