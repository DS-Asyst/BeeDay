# Sprint 2.1 — User and Character Domain Refactor

## Result

The former `Profile` concept is no longer part of the active domain or application architecture.

- `User` owns account information: name, email, password hash, language, theme and account state.
- `Character` owns gameplay identity: nickname, class and avatar.
- `User` and `Character` have a one-to-zero-or-one relationship.
- Activities are owned by `UserId`.
- A new data store starts with no user and no character.
- The initial character-creation flow creates a provisional local `User` and its `Character` atomically. Authentication and real registration credentials belong to Sprint 2.2.

## Backward compatibility

Old JSON files containing `profile` are migrated through a private serialization snapshot. The legacy Profile entity and handlers are not restored.

Old Daily data without a user is assigned to a migration-only user. A genuinely empty data file remains empty.

## Stale-file protection

`LevelUp.Application.csproj` explicitly excludes `Features/Profiles/**/*.cs`. This prevents obsolete handlers left by an in-place ZIP extraction from breaking the build.

## Test reset

The distributed ZIP contains an empty `src/LevelUp.Web/Data` directory. To reset it again:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Reset-TestData.ps1
```
