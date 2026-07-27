# Production Hosting

LevelUp targets IIS on Windows with external persistent storage.

## Standard paths

```text
Application publish:      C:\Apps\LevelUp
External runtime root:    C:\Apps\LevelUp-Data
JSON data:                C:\Apps\LevelUp-Data\Data
Data Protection keys:     C:\Apps\LevelUp-Data\DataProtection-Keys
Generated emails:         C:\Apps\LevelUp-Data\Emails
Logs:                     C:\Apps\LevelUp-Data\Logs
Application backups:      C:\Apps\LevelUp-Backups\Application
Data snapshots:           C:\Apps\LevelUp-Backups\Data
```

The IIS application-pool identity requires Modify permission on the external runtime directories.

## Runtime requirements

- Production environment variables;
- explicit `AllowedHosts` values;
- an HTTPS public base URL for identity emails;
- absolute external storage and Data Protection key paths;
- valid Resend configuration when production email delivery is enabled;
- reviewed forwarded-header proxy or network configuration.

## Health endpoints

- `/health/live`: process liveness;
- `/health/ready`: deployment readiness dependencies;
- `/health`: complete health report.

Deployment validates readiness and rolls back application binaries when the new version does not become ready. Runtime data remains external and is not replaced by application rollback.
