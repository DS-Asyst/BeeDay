# LevelUp

LevelUp is a personal productivity application with RPG-inspired character progression. It is built with ASP.NET Core, Blazor Server, and .NET 10.

The repository uses a layered architecture so domain rules, use cases, infrastructure, and presentation can evolve independently.

## Current product scope

Implemented modules and platform capabilities:

- user registration, cookie authentication, email confirmation, and password recovery;
- account management and character onboarding;
- Daily management for habits, recurring tasks, todos, and projects;
- Inventory management for wallets, transactions, and tags;
- RPG experience model, experience curve, idempotent reward pipeline, and character XP interface;
- JSON persistence with atomic writes, backups, and recovery;
- reusable Blazor design-system components;
- CI validation and IIS production deployment with rollback;
- automated tests for Domain, Application, Infrastructure, and Web.

## Architecture

```text
LevelUp.Domain
    ↑
LevelUp.Application
    ↑
LevelUp.Infrastructure
    ↑
LevelUp.Web
```

- **Domain** owns entities, value objects, domain events, experience progression, and business invariants.
- **Application** owns commands, queries, validation, handlers, contracts, and use-case orchestration.
- **Infrastructure** implements JSON persistence, password hashing, email delivery, caching, auditing, health dependencies, and background services.
- **Web** hosts the Blazor Server UI, authentication endpoints, layouts, feature state, diagnostics, and dependency-injection composition.

See [Architecture](docs/ARCHITECTURE.md) for dependency and ownership rules.

## Repository structure

```text
.github/                 Pull-request template and GitHub Actions workflows
docs/                    Maintained project documentation
scripts/                 Operational and local-development scripts
src/                     Production projects
tests/                   Automated test projects
Directory.Build.props    Shared .NET build settings
Directory.Packages.props Central package version management
LevelUp.slnx             Solution definition
```

## Requirements

- .NET 10 SDK
- supported modern browser
- PowerShell 7 for the provided Windows scripts

## Local development

```bash
dotnet restore LevelUp.slnx
dotnet format LevelUp.slnx --verify-no-changes --no-restore
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Development configuration is stored in `src/LevelUp.Web/appsettings.json`. Local application data is written under `src/LevelUp.Web/Data` and is ignored by Git except for `.gitkeep`.

## Quality gate

Run before opening or merging a pull request:

```bash
git status
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx --configuration Release --warnaserror
dotnet test LevelUp.slnx --configuration Release
```

## Branch strategy

- `hmg`: integration and validation
- `prd`: production
- temporary work branches: `feature/*`, `fix/*`, `refactor/*`, `docs/*`, or `chore/*`

Example:

```bash
git switch hmg
git pull origin hmg
git switch -c feature/character-progression
```

Changes should reach `prd` only after validation in `hmg`.

## Configuration and secrets

Do not commit production secrets or runtime data. Use environment variables, user secrets, or the deployment platform's secret store.

Production data, backups, Data Protection keys, generated emails, and logs live outside the publish directory under `C:\Apps\LevelUp-Data`. See [Production](docs/PRODUCTION.md) and [CI/CD](docs/CI_CD.md).

## Documentation

- [Pixel Icon Library](docs/PIXEL_ICON_LIBRARY.md)

- [Architecture](docs/ARCHITECTURE.md)
- [Authentication](docs/AUTHENTICATION.md)
- [CI/CD](docs/CI_CD.md)
- [Development](docs/DEVELOPMENT.md)
- [Domain](docs/DOMAIN.md)
- [Persistence](docs/PERSISTENCE.md)
- [Production](docs/PRODUCTION.md)
- [Roadmap](docs/ROADMAP.md)
- [User Interface](docs/UI.md)

Documentation is maintained in English and must be updated in the same change as the implementation it describes.

## Experience progression

The finalized XP and Level Up pipeline is documented in [`docs/EXPERIENCE.md`](docs/EXPERIENCE.md).

### Activity attributes

Daily activities can be classified as Strength, Dexterity, Intelligence, Wisdom, Vitality, or Charisma. The Daily dashboard supports attribute-aware search, filtering, and optional attribute sorting while preserving manual card ordering as the default.
