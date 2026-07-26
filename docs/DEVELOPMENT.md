# Development

## Branches

- `hmg`: integration and validation
- `prd`: production
- Work branches: `feature/*`, `fix/*`, `refactor/*`, `docs/*`, or `chore/*`

Create a repository cleanup branch with:

```bash
git switch hmg
git pull origin hmg
git switch -c chore/repository-cleanup
```

## Restore, Format, Build, and Test

```bash
dotnet restore
dotnet format --verify-no-changes
dotnet build
dotnet test
```

## Run the Web Application

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

## Repository Hygiene

Do not commit:

- `.git`, `.vs`, `.vscode`, or `.idea` metadata
- `bin` or `obj` directories
- logs, temporary files, test results, or coverage output
- local backups
- generated development emails
- production JSON data
- secrets or environment-specific credentials

Keep documentation in English and update it in the same pull request as the implementation it describes.
