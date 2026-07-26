# CI/CD Hardening

This document describes the LevelUp validation and production deployment pipelines.

## Continuous integration

`.github/workflows/ci.yml` runs for pushes to `hmg`, pull requests targeting `hmg` or `prd`, and manual executions.

The workflow performs, in order:

1. checkout;
2. .NET 10 setup;
3. dependency restore;
4. formatting verification;
5. Release build with warnings treated as errors;
6. the complete automated test suite;
7. Release publish;
8. publish artifact validation;
9. upload of test results and the validated publish artifact.

A failed formatting check, build, test, publish, or artifact validation fails the workflow. CI runs for the same ref cancel older in-progress runs.

## Production deployment

`.github/workflows/deploy-prd.yml` runs for pushes to `prd` and manual executions.

The `validate` job restores, formats, builds, tests, and publishes on a GitHub-hosted runner. The `deploy` job cannot start unless `validate` succeeds. The exact artifact produced by the validated job is downloaded by the self-hosted runner; production does not rebuild source code.

Production uses this concurrency group:

```text
levelup-production
```

`cancel-in-progress` is disabled, so a newer deployment waits instead of interrupting a running deployment.

The `production` GitHub Environment should require reviewer approval and restrict deployments to the `prd` branch.

## Required GitHub secrets

Configure these secrets in the `production` Environment, not as repository files:

| Secret | Required | Purpose |
| --- | --- | --- |
| `LEVELUP_PUBLIC_BASE_URL` | Yes | Public HTTPS origin, such as `https://levelup.example.com`. |
| `LEVELUP_RESEND_API_KEY` | Yes | Production Resend API key. |
| `LEVELUP_RESEND_FROM_ADDRESS` | Yes | Verified production sender address. |
| `LEVELUP_RESEND_FROM_NAME` | No | Sender display name. Defaults to `LevelUp`. |
| `LEVELUP_ALLOWED_HOSTS` | Yes | Semicolon-separated production hosts without wildcard values. |

The workflow validates presence of all required secrets before changing IIS. Secret values are passed as process environment variables and are never printed intentionally.

## Self-hosted runner requirements

The production runner must:

- run on Windows x64;
- have network access to the IIS server when it is not installed locally;
- have the IIS `WebAdministration` PowerShell module;
- have permission to stop and start the `LevelUp` site and `LevelUpPool` application pool;
- have Modify permission under `C:\Apps`;
- be dedicated or tightly controlled because deployment jobs execute repository scripts with elevated filesystem and IIS access.

Do not allow untrusted pull-request code to execute on this runner.

## Deployment and backup sequence

The hardened deployment script performs:

1. validation of the downloaded publish artifact;
2. creation and permission validation of external runtime directories;
3. backup of the current application;
4. backup of persistent JSON data;
5. IIS stop;
6. application-pool environment configuration;
7. replacement of application binaries only;
8. IIS start;
9. readiness health checks with retries;
10. automatic application rollback when readiness fails.

Application backups are stored under:

```text
C:\Apps\LevelUp-Backups\Application
```

Data backups are stored under:

```text
C:\Apps\LevelUp-Backups\Data
```

Persistent runtime data remains under `C:\Apps\LevelUp-Data` and is never cleared during deployment or rollback.

## Health check and rollback

The script checks:

```text
http://127.0.0.1/health/ready
```

with the expected IIS host header. It retries six times with five-second intervals. A non-200 response or connection failure triggers rollback.

Rollback stops IIS, restores the pre-deployment application backup, starts IIS, and repeats the readiness check. Persistent data is not rolled back automatically because doing so could discard valid writes. The pre-deployment data snapshot is retained for manual recovery.

## Operational validation

Before merging into `prd`:

```bash
dotnet format --verify-no-changes
dotnet build --configuration Release --warnaserror
dotnet test --configuration Release
```

After a successful deployment, confirm:

- the workflow used the expected commit SHA;
- `/health/ready` is healthy;
- application and data backup directories exist;
- `C:\Apps\LevelUp-Data` was preserved;
- no secret value was written to workflow logs.
