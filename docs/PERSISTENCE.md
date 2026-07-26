# Persistence

The current persistence provider is JSON and is implemented in `LevelUp.Infrastructure`.

## Storage

Default configuration:

- Directory: `src/LevelUp.Web/Data`
- Data file: `LevelUpBD.json`
- Backup directory: `Backups`

Application data, generated development emails, backups, and the production JSON database are excluded from version control.

## Reliability Features

The persistence implementation includes:

- Serialized access through a storage gate
- Atomic file replacement
- Configurable backup creation
- Backup retention
- Recovery support
- Dedicated exceptions for corruption, access failures, and restore failures
- Readiness health checks

## Boundaries

Application code depends on `ILevelUpRepository`. Domain and Application do not depend on JSON APIs or file-system paths. This boundary allows the storage provider to be replaced later without changing core business rules.

## Operational Guidance

- Preserve the `Data` directory during deployment.
- Back up production data before migrations or manual intervention.
- Never include a production data file in source archives or commits.
- Store environment-specific paths and secrets outside source control.
