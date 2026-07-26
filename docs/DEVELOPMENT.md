# Development

## Branches

- `hmg`: integration and validation
- `prd`: production
- work branches: `feature/*`, `fix/*`, `refactor/*`, `docs/*`, or `chore/*`

Create work from the latest `hmg`:

```bash
git switch hmg
git pull origin hmg
git switch -c feature/example
```

## Restore, format, build, and test

```bash
dotnet restore LevelUp.slnx
dotnet format LevelUp.slnx --verify-no-changes --no-restore
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
```

Release validation:

```bash
dotnet build LevelUp.slnx --configuration Release --warnaserror
dotnet test LevelUp.slnx --configuration Release
```

## Run the application

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

The default HTTP and HTTPS addresses are defined in `src/LevelUp.Web/Properties/launchSettings.json`.

## Local data

Development data is stored under `src/LevelUp.Web/Data` by default. To reset it:

```powershell
pwsh ./scripts/Reset-TestData.ps1
```

This script deletes local development data. It must never be used against a production data directory.

## Repository hygiene

Do not commit:

- `.git`, `.vs`, `.vscode`, or `.idea` metadata;
- `bin`, `obj`, `artifacts`, or publish output;
- logs, temporary files, test results, or coverage output;
- local backups or generated development emails;
- `LevelUpBD.json` or other runtime data;
- credentials, tokens, API keys, or environment-specific secrets;
- sprint-only migration helpers after their purpose has ended.

Keep only maintained documentation. Remove obsolete sprint notes and duplicate guides once their durable content has been consolidated.

## Pull requests

A pull request should:

- explain the behavior changed;
- identify affected layers;
- include tests for business rules and important UI contracts;
- update documentation in the same change;
- pass the repository quality gate;
- avoid unrelated formatting or generated-file changes.
