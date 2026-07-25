# Development and Deployment

## Local workflow

```bash
dotnet restore LevelUp.slnx
dotnet build LevelUp.slnx --configuration Debug
dotnet test LevelUp.slnx --configuration Debug
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Use `develop` for integrated development and short-lived `feature/...` branches for changes. Open pull requests into `develop`; promote validated releases from `develop` to `main`.

## Configuration

Base storage settings are in `src/LevelUp.Web/appsettings.json`. Environment-specific logging is in:

- `appsettings.Development.json`
- `appsettings.Production.json`

Do not store credentials in these files. Use environment variables, GitHub environment secrets or the host secret provider.

Nested configuration can be overridden with environment variables, for example:

```text
LevelUp__Storage__Directory=D:\LevelUp\Data
```

## Runtime data

The application creates these files at runtime:

```text
Data/LevelUpBD.json
Data/LevelUpEvents.ndjson
Data/Backups/*.json
```

They are ignored by Git. Back up production data outside the deployment directory before destructive deployment changes.

For local test reset:

```powershell
./scripts/Reset-TestData.ps1
```

Review the script parameters before execution.

## Quality gates

Before opening a pull request:

```bash
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx --configuration Release
dotnet test LevelUp.slnx --configuration Release
```

The central build configuration enables nullable analysis, .NET analyzers, code-style enforcement and deterministic builds.

## GitHub Actions

### Continuous integration

`.github/workflows/ci.yml` runs on:

- Push to `develop`
- Pull request to `develop` or `main`
- Manual dispatch

It restores, builds and tests with .NET 10 on Windows.

### Production deployment

`.github/workflows/deploy-prd.yml` runs on push to `main` or manual dispatch. It:

1. Validates restore, build and tests on a GitHub-hosted runner.
2. Checks out the repository on the self-hosted Windows runner.
3. Publishes `LevelUp.Web` to a temporary directory.
4. Validates required output files.
5. Runs `scripts/Deploy-LevelUp.ps1` for IIS deployment.

Recommended repository controls:

- Require CI before merging
- Protect `main`
- Use a protected GitHub Environment for production
- Restrict who can trigger deployments
- Keep the self-hosted runner dedicated and patched
- Store deployment paths and credentials outside source control

## IIS prerequisites

- Windows Server with IIS
- ASP.NET Core Hosting Bundle matching .NET 10
- Application pool configured for `No Managed Code`
- Filesystem permissions for the application identity
- Persistent writable storage directory for JSON data
- HTTPS binding and certificate

The writable data directory should preferably live outside the publish directory so application deployment cannot overwrite it.

## Release checklist

1. Merge reviewed changes into `develop`.
2. Confirm CI is green.
3. Validate data migration against a copied production JSON file.
4. Merge `develop` into `main`.
5. Observe the production workflow.
6. Check `/health/live`, `/health/ready` and `/health`.
7. Verify onboarding, Daily, project and My Account flows.
8. Confirm runtime data and backup rotation.

## Design system foundation

Reusable UI primitives live under `src/LevelUp.Web/Components/DesignSystem` and their shared styles live in `src/LevelUp.Web/wwwroot/css/design-system.css`.

Use these components before introducing feature-specific markup:

- `LevelUpButton` for primary, secondary and destructive actions.
- `LevelUpCard` for bordered surfaces, including padded, muted and interactive variants.
- `LevelUpInput`, `LevelUpSelect`, `LevelUpTextArea`, `LevelUpDateInput` and `LevelUpCheckbox` for forms.
- `LevelUpPageHeader` and `LevelUpSectionHeader` for page and section hierarchy.
- feedback primitives such as loading, empty state, skeleton, toast and confirmation dialog.

Feature CSS may control layout, but shared interaction, typography, focus, spacing and visual variants belong to the design system.

## Sprint 3.6 UI polish layer

`wwwroot/css/polish.css` is loaded after `pixel-ui.css` and is the final cross-component refinement layer. Place global spacing, responsive, accessibility and control-size rules there. Component-specific visual rules should remain in the component stylesheet or the relevant Design System stylesheet.

The icon component renders pixel rectangles declaratively in Razor. Do not reintroduce manually incremented `RenderTreeBuilder` sequence numbers; they produce `ASP0006` analyzer warnings and make source-order reasoning harder.
