# Production Configuration

This document defines the required production configuration for LevelUp when hosted on IIS.

## Runtime directories

Application binaries are deployed to:

```text
C:\Apps\LevelUp
```

Persistent runtime state is stored outside the publish directory:

```text
C:\Apps\LevelUp-Data\
├── Data\
│   └── Backups\
├── DataProtection-Keys\
├── Emails\
└── Logs\
```

The IIS application pool identity `IIS AppPool\LevelUpPool` requires `Modify` permission on these directories. The deployment script creates the directories and applies the permission.

Never store the production database, backups, Data Protection keys, generated emails, or logs inside the deployment directory. A deployment may replace every file under `C:\Apps\LevelUp`.

## Required environment variables

ASP.NET Core maps double underscores to configuration separators. Configure these variables on the server or application pool. Do not commit their values.

| Variable | Required | Description |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT=Production` | Yes | Activates production configuration and validation. |
| `DOTNET_ENVIRONMENT=Production` | Yes | Keeps the generic host environment aligned. |
| `LevelUp__IdentityEmail__PublicBaseUrl` | Yes | Public HTTPS origin, for example `https://levelup.example.com`. |
| `LevelUp__Email__Resend__ApiKey` | Yes | Resend API key. |
| `LevelUp__Email__Resend__FromAddress` | Yes | Verified sender address. |
| `LevelUp__Email__Resend__FromName` | No | Sender display name. Defaults to `LevelUp`. |
| `AllowedHosts` | Yes | Semicolon-separated production host names. Must not contain `*`. |

The following values have safe server defaults in `appsettings.Production.json`, but may be overridden:

| Variable | Default |
| --- | --- |
| `LevelUp__Storage__Directory` | `C:\Apps\LevelUp-Data\Data` |
| `LevelUp__Hosting__DataProtectionKeysDirectory` | `C:\Apps\LevelUp-Data\DataProtection-Keys` |
| `LevelUp__Hosting__ForwardedHeaders__Enabled` | `true` |
| `LevelUp__Hosting__ForwardedHeaders__ForwardLimit` | `1` |

Array values use numeric indexes, for example:

```text
LevelUp__Hosting__ForwardedHeaders__KnownProxies__0=127.0.0.1
LevelUp__Hosting__ForwardedHeaders__KnownProxies__1=::1
```

Only add addresses or CIDR networks belonging to trusted reverse proxies. Forwarded headers from unknown proxies are ignored.

## Resend

Production enables Resend and disables development email capture. Startup fails when the API key or verified sender address is missing.

The application does not log recipient addresses, message bodies, API responses, tokens, or API keys. Resend failures are recorded only with the HTTP status code.

## HTTPS and proxy handling

Production uses:

- HSTS;
- HTTPS redirection;
- secure authentication cookies;
- explicit allowed hosts;
- forwarded `For`, `Proto`, and `Host` headers from trusted proxies only;
- symmetric forwarded-header validation;
- a maximum forwarding depth of one by default.

The public base URL must use HTTPS. Startup fails for an HTTP or malformed public URL.

## Data Protection

Authentication cookies and antiforgery tokens depend on ASP.NET Core Data Protection keys. Keys are persisted to `C:\Apps\LevelUp-Data\DataProtection-Keys`, use the stable application name `LevelUp`, and are protected with Windows DPAPI at machine scope.

Preserving this directory prevents users from being signed out after every deployment. Back up and protect it like other application secrets. In a multi-server deployment, all instances must share the same protected key repository.

## Logging

Application logs use structured JSON on standard output. IIS stdout startup logs are written to:

```text
C:\Apps\LevelUp-Data\Logs
```

Restrict access to this directory and define an operating-system retention policy. Logs must not contain credentials, tokens, cookie values, email contents, Resend keys, or full third-party response bodies.

## Health checks

- `/health/live`: process liveness; does not access dependencies.
- `/health/ready`: readiness, including writable and readable JSON storage.
- `/health`: complete diagnostic health report.

The deployment script validates `/health/ready` after starting IIS. Expose only the minimum endpoint required by infrastructure. Restrict the complete `/health` endpoint at the reverse proxy or firewall when it is not required publicly.

## Deployment validation

Before merging into `prd`:

```bash
dotnet format --verify-no-changes
dotnet build --configuration Release
dotnet test --configuration Release
```

After deployment:

```powershell
Invoke-WebRequest https://levelup.example.com/health/live
Invoke-WebRequest https://levelup.example.com/health/ready
```

Verify that data, backups, Data Protection keys, and logs remain under `C:\Apps\LevelUp-Data` after a second deployment.
