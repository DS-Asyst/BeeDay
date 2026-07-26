# LevelUp

LevelUp is a personal productivity and RPG-inspired progression application built with ASP.NET Core, Blazor Server, and .NET 10.

The project separates business rules, application use cases, infrastructure concerns, and presentation into independent layers so features can evolve without coupling the domain to the UI or the current JSON storage provider.

## Current Scope

Implemented areas include:

- User registration and cookie-based authentication
- Email confirmation and password recovery flows
- Character creation and onboarding
- Daily productivity management with habits, recurring tasks, todos, and projects
- Inventory management with wallets, transactions, and tags
- Centralized RPG experience domain with derived level progression and source history
- JSON persistence with atomic writes, backups, and recovery support
- Reusable Blazor design-system components
- Automated tests for Domain, Application, Infrastructure, and Web layers

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

- `LevelUp.Domain`: entities, value objects, domain events, enums, experience progression, and business rules.
- `LevelUp.Application`: commands, queries, validators, handlers, security contracts, and orchestration.
- `LevelUp.Infrastructure`: JSON persistence, password hashing, email delivery, caching, auditing, background services, and health checks.
- `LevelUp.Web`: Blazor Server UI, authentication endpoints, layouts, feature components, and application composition.

Dependency direction is enforced toward the Domain layer. The Web project acts as the composition root.

## Repository Structure

```text
.github/                 Pull request and workflow configuration
docs/                    Maintained project documentation
scripts/                 Local development and deployment scripts
src/                     Production projects
tests/                   Automated test projects
Directory.Build.props    Shared .NET build configuration
Directory.Packages.props Central package version management
LevelUp.slnx             Solution definition
```

## Requirements

- .NET 10 SDK
- A supported browser
- PowerShell 7 or a POSIX-compatible shell for optional scripts

## Local Development

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

The default development URL is defined in `src/LevelUp.Web/Properties/launchSettings.json`.

## Configuration

Application configuration is stored under `src/LevelUp.Web/appsettings*.json`.

Do not commit production secrets. Supply sensitive values through environment variables, user secrets, or the deployment platform's secret store.

The default development persistence provider stores application data under `src/LevelUp.Web/Data`. Production stores data, backups, Data Protection keys, generated emails, and logs outside the publish directory under `C:\Apps\LevelUp-Data`. See the production guide before deploying.

## Quality Gate

Run the following checks before opening or merging a pull request:

```bash
git status
dotnet format --verify-no-changes
dotnet build
dotnet test
```

## Branch Strategy

- `hmg`: integration and validation branch
- `prd`: production branch
- Work branches: use a descriptive prefix such as `feature/`, `fix/`, `refactor/`, `docs/`, or `chore/`

Example:

```bash
git switch hmg
git pull origin hmg
git switch -c chore/repository-cleanup
```

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [Authentication](docs/AUTHENTICATION.md)
- [Development](docs/DEVELOPMENT.md)
- [Domain](docs/DOMAIN.md)
- [Persistence](docs/PERSISTENCE.md)
- [Production Configuration](docs/PRODUCTION.md)
- [Roadmap](docs/ROADMAP.md)
- [User Interface](docs/UI.md)

Documentation must remain in English and reflect the current implementation.

## Operations documentation

- [Production configuration](docs/PRODUCTION.md)
- [CI/CD hardening](docs/CI_CD.md)


### Inventory quality baseline

The Inventory module includes responsive transaction and tag management, in-memory search and filters for the current JSON persistence, guarded interaction states, accessible empty/loading feedback, and bUnit coverage for its primary UI contracts.

Validate changes with:

```bash
dotnet format --verify-no-changes
dotnet build
dotnet test
```

## RPG experience foundation

Character XP is centralized in the Domain layer. `TotalExperience` is persisted as the single source of truth, while level, current-level progress, and XP remaining are derived through `ExperienceCurve`. Each reward records its origin in an `ExperienceTransaction`.

Activity modules must not write XP fields directly. Future Application handlers should calculate an `ExperienceReward`, create an `ExperienceSource`, and invoke `Character.AddExperience`.
