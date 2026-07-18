# Schema 7 Persistence Baseline

This branch now uses a single SQLite-compatible EF Core migration for the current Schema 7 model.

## Changes

- Squashed the previous JSON-to-relational and Schema 7 migrations into `20260718000000_InitialSchema7`.
- Removed the unsupported SQLite `DropColumnOperation`.
- Creates `Habits` directly without `DurationInMinutes`.
- Creates `Tasks` and `Todos` directly in the initial relational schema.
- Preserves all current relational tables and foreign keys required by the Console application.
- Keeps the Git repository on `refactor/schema-7-fixes`.

## Important

Because the migration history was reset, existing development databases created by the old migration chain should be deleted and recreated before running the application or tests.

```bash
dotnet clean
dotnet build
dotnet test
```
