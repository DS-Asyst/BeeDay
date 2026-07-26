# Persistence

The current persistence provider is JSON and is implemented in `LevelUp.Infrastructure` behind Application contracts.

## Development storage

Default development configuration:

```text
Directory: src/LevelUp.Web/Data
File:      LevelUpBD.json
Backups:   src/LevelUp.Web/Data/Backups
```

The directory contents are ignored by Git except for `.gitkeep`.

## Production storage

Production uses an absolute external path:

```text
C:\Apps\LevelUp-Data\Data\LevelUpBD.json
C:\Apps\LevelUp-Data\Data\Backups
```

Runtime data must remain outside `C:\Apps\LevelUp`, which is the replaceable application publish directory.

## Write and recovery behavior

The JSON provider is configured to:

- serialize through a single repository boundary;
- write atomically through a temporary file and replacement step;
- create a backup before save when enabled;
- retain a configured number of backups;
- recover from a valid backup when the primary file cannot be read and recovery is enabled;
- validate restored domain state before returning it to the application.

Development uses indented JSON and a retention of 10 backups. Production uses compact JSON and a retention of 20 backups.

## Authoritative state

Persisted data includes users, characters, Daily entities, Inventory entities, tokens, total character XP, and experience transaction history.

Calculated values such as wallet balance, current character level, current-level XP, and remaining XP should not be persisted independently when they can be derived reliably.

## Deployment safety

The production deployment script:

- backs up the current application;
- snapshots persistent JSON data;
- replaces only application binaries;
- leaves external runtime data in place;
- rolls back application binaries when readiness checks fail.

Data is not automatically rolled back because doing so could discard valid writes made during deployment. The snapshot is retained for manual recovery.

## Repository rules

Never commit:

- `LevelUpBD.json`;
- generated backups;
- generated development emails;
- production runtime directories;
- local persistence exports containing user data.
