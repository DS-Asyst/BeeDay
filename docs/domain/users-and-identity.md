# Users and Identity

A User owns account identity, profile, preferences, active state, experience, and identity tokens. There is no separate character or player entity — profile and progression data live directly on `User`.

## Invariants

- Email addresses, user names, and nicknames use validated value objects.
- Password material is not stored as plaintext.
- Identity tokens have an explicit type and lifecycle.
- Inactive or missing users cannot retain a valid authenticated principal.
- User-scoped operations must enforce multi-user isolation through the current-user context and ownership checks.

## Profile

A User's full name (`Name`) is required from registration. `Nickname` and `Avatar` are set once, together, through `User.CompleteProfile` (invoked via `LevelUpData.CompleteUserProfile`, which also enforces nickname uniqueness across users); the nickname cannot be changed afterward. `HasProfile` reports whether a user has completed this step — the application routes a user without a profile to profile creation before Daily or the Wallet.

## Preferences

The current domain supports language and theme preferences through `UserLanguage` and `UserTheme`.

## Identity operations

Application use cases coordinate registration, authentication, confirmation, reset, account updates, preference changes, and password changes. Domain state changes remain independent from HTTP and provider details.
