# Sprint 2 — Stage 1: Identity Domain

## Scope

This stage introduces the domain model required by email confirmation and password recovery without implementing application handlers, email delivery, token generation, or web pages.

## Delivered

- `User.IsEmailConfirmed` and `User.EmailConfirmedAtUtc`.
- Idempotent `User.ConfirmEmail` behavior.
- `UserToken` entity with ownership, purpose, hash, creation, expiration, use, and revocation state.
- `UserTokenType` for email confirmation and password reset.
- Token ownership and integrity rules in `LevelUpData`.
- User-scoped token snapshots.
- Schema version 5 and migration behavior that preserves access for accounts created before email confirmation existed.
- Domain tests for confirmation, expiration, single use, purpose isolation, revocation, and ownership.

## Explicitly deferred

- Token generation and hashing implementations.
- Commands and handlers.
- Email provider integration.
- Confirmation and password reset pages.
- Login enforcement for unconfirmed accounts.
