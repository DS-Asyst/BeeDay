# Sprint 2.1 — User Domain

## Implemented

- Replaced the mixed `Profile` model with separate `User` and `Character` aggregates.
- Added one-to-zero-or-one relationship from `User` to `Character`.
- Added unique, normalized email and immutable character nickname rules.
- Added language and theme preferences to `User`.
- Added password-hash storage to `User` without implementing authentication.
- Added `UserId` ownership to Daily activities.
- Upgraded JSON persistence to schema version 3.
- Added automatic migration from the legacy `profile` JSON object.
- Added user and character application commands/queries.
- Kept the current character-creation UI functional against the new domain.

## Transitional behavior

Until Sprint 2.2 introduces authentication, the persistence root contains a `CurrentUserId`. Existing local data is migrated to a bootstrap user with the reserved email `local-user@levelup.invalid`. Authentication will replace this transitional current-user selection with the authenticated principal.

## Domain ownership

- `User`: name, email, password hash, language, theme, account status and timestamps.
- `Character`: user relationship, nickname, class, avatar and character timestamps.
- Daily entities: owned by `UserId`, not `CharacterId`.


## Sprint 2.1 review

- Removed the legacy `Features/Profiles` application module.
- Removed the legacy `Profile`, `ProfileName`, and `ProfileNickname` domain types.
- Kept legacy JSON migration through a private persistence snapshot only.
- Renamed the character creation web feature and route to `/character/create`.
- Added `scripts/Apply-Sprint21Review.ps1` for checkouts where obsolete files remain after extracting an archive over an existing folder.
