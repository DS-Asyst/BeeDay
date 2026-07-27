# Persistence Architecture

LevelUp currently persists application state as JSON through the `ILevelUpRepository` contract and the Infrastructure JSON implementation.

## Components

- `JsonStoragePaths`: resolves data, backup, and related paths;
- `JsonSerializerOptionsFactory`: central serializer configuration;
- `JsonFileReader` and `JsonFileWriter`: file operations;
- `JsonAtomicFileCommitter`: replacement strategy for completed writes;
- `JsonStorageGate`: serialized access to shared storage;
- `JsonStorageInitializer`: startup initialization and recovery preparation;
- `JsonBackupService`: data snapshots and restore support;
- `JsonLevelUpRepository`: Application-facing repository implementation.

## Rules

- Web and Application depend on repository contracts, not concrete JSON classes.
- Writes must be atomic from the application's perspective.
- Shared file access must pass through the storage gate.
- Backups and runtime data are external operational concerns and must not be committed.
- Missing optional fields must remain backward-compatible where possible.
- Persist authoritative state only; calculate stable derived values.

## Development and production locations

Development uses the configured directory under `src/LevelUp.Web/Data` by default.

Production deployment is designed to keep persistent state outside the publish directory, normally under:

```text
C:\Apps\LevelUp-Data\Data
```

This separation prevents application replacement and rollback from deleting user data.
