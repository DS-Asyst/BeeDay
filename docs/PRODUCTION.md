# Production

LevelUp production hosting targets IIS on Windows and uses external runtime storage.

## Paths

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

The IIS application-pool identity must have Modify permission on the external runtime directories.

## Required production configuration

- `ASPNETCORE_ENVIRONMENT=Production`
- `DOTNET_ENVIRONMENT=Production`
- explicit `AllowedHosts` without wildcard values;
- absolute HTTPS `LevelUp:IdentityEmail:PublicBaseUrl`;
- absolute external `LevelUp:Storage:Directory`;
- absolute external `LevelUp:Hosting:DataProtectionKeysDirectory`;
- Resend enabled with a valid API key and verified sender;
- known proxies or networks configured when forwarded headers are enabled.

The application fails startup outside Development when required production safety conditions are not met.

## Data Protection

Production keys are persisted outside the publish directory. On Windows, keys are protected with machine-level DPAPI. Keeping the same key directory is necessary for authentication cookies and protected tokens to survive deployments.

## Health endpoints

The deployment workflow validates the readiness endpoint:

```text
/health/ready
```

A failed readiness check triggers application rollback.

## Deployment

Pushes to `prd` trigger `.github/workflows/deploy-prd.yml`. The workflow validates and publishes on a GitHub-hosted runner, then deploys the exact validated artifact through a controlled self-hosted Windows runner.

The deployment script preserves external data, creates application and data backups, configures IIS environment variables, replaces application files, checks readiness, and restores the previous application version if the new version is unhealthy.

See [CI/CD](CI_CD.md) for runner and secret requirements.
