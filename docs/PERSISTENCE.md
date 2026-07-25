# JSON Persistence

## Purpose

The JSON store is the current persistence mechanism for LevelUp. It is intentionally isolated behind `ILevelUpRepository` so application handlers do not depend on file-system or serializer details.

## Components

- `JsonLevelUpRepository`: coordinates load, save, update, backup recovery, validation and logging.
- `JsonStorageGate`: singleton synchronization for the complete read-modify-write cycle.
- `JsonStorageInitializer`: creates the storage and backup directories in one place.
- `JsonFileReader`: deserializes, validates and normalizes compatible documents.
- `JsonFileWriter`: writes a complete document to a new temporary file.
- `JsonAtomicFileCommitter`: replaces the primary file and cleans temporary files.
- `JsonBackupService`: creates validated backups, applies retention and restores the latest valid copy.
- `JsonStoragePaths`: resolves every persistence path from configuration.

## Write flow

1. Acquire the shared storage gate.
2. Normalize and validate `LevelUpData`.
3. Serialize to a unique temporary file using write-through I/O.
4. Read and validate the temporary file.
5. Back up the current primary file, when enabled.
6. Replace the primary file with the validated temporary file.
7. Remove any remaining temporary file.
8. Release the gate in `finally`.

The lock covers the entire read-modify-write transaction. Protecting only the final file write would allow concurrent requests to overwrite each other's changes.

## Read flow

- A missing primary file creates a valid empty, versioned document.
- Compatible missing or null collections are normalized centrally by `LevelUpData.EnsureValidState()`.
- Invalid primary JSON triggers backup recovery when enabled.
- If neither the primary file nor a backup is valid, a controlled persistence exception is raised.

## Schema compatibility

`LevelUpData.SchemaVersion` is currently `5`. Compatibility and migration code lives in `LevelUpData.Persistence.cs`, separated from the aggregate's normal commands and queries.

Unknown JSON properties are ignored by the serializer. Older known shapes are normalized or migrated before the document is returned to the application.

## Configuration

Section: `LevelUp:Storage`

- `Directory`: storage directory relative to the content root.
- `FileName`: primary JSON file name.
- `BackupDirectory`: backup directory below the storage directory.
- `BackupRetention`: number of validated backups retained.
- `CreateBackupBeforeSave`: creates a backup before replacing an existing primary file.
- `RecoverFromBackup`: enables automatic recovery from the latest valid backup.
- `WriteIndented`: controls development-friendly formatting.

## Testing rules

Persistence tests use a unique directory below the operating-system temporary path. They must never read from or write to the application's real `Data` directory.

Coverage includes:

- round-trip serialization;
- initial document creation;
- compatibility normalization;
- backup retention and recovery;
- concurrent updates without lost writes;
- lock release after exceptions;
- temporary-file cleanup;
- storage health checks.

## SQL Server migration boundary

A future SQL Server implementation should preserve the application-facing repository contract while replacing the JSON-specific infrastructure. Domain entities and application handlers must not acquire file-system dependencies during the migration.
