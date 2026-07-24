# LevelUp

LevelUp is a personal productivity application that organizes habits, recurring tasks, projects and project To-Dos in a gamified daily workspace.

The current version is an ASP.NET Core application with an interactive Blazor Server interface, a layered architecture and local JSON persistence. The codebase is prepared for future replacement of the persistence provider without coupling the domain or application layers to a database technology.

## Current capabilities

- User account and preferences domain
- One character per user, with immutable nickname after creation
- Character onboarding and class selection
- Daily dashboard with search, filters and drag-and-drop ordering
- Habits with positive/negative tracking and difficulty
- Independent recurring tasks
- Projects that aggregate mandatory project To-Dos
- Automatic project status/progress derived from its To-Dos
- Atomic JSON writes, rotating backups and recovery
- Domain-event auditing and in-memory dashboard caching
- Liveness, readiness and storage health checks
- Automated build and test workflows
- IIS production deployment through a self-hosted GitHub Actions runner

## Technology stack

- .NET 10
- ASP.NET Core
- Blazor Server / Razor Components
- C# with nullable reference types enabled
- MediatR
- FluentValidation
- System.Text.Json
- xUnit v3, bUnit and AngleSharp
- GitHub Actions

## Architecture

```text
Browser
   |
   v
LevelUp.Web              Presentation, UI state and HTTP pipeline
   |
   v
LevelUp.Application      Commands, queries, validation and orchestration
   |
   v
LevelUp.Domain           Entities, value objects, events and invariants
   ^
   |
LevelUp.Infrastructure   JSON storage, backups, cache, audit and workers
```

Dependency direction:

```text
Domain <- Application <- Infrastructure
   ^            ^              ^
   +------------+--------------+--- Web composition root
```

`LevelUp.Web` is the composition root and references all runtime projects. `LevelUp.Domain` has no project or package dependencies.

Detailed documentation:

- [Architecture](docs/ARCHITECTURE.md)
- [Code review and technical findings](docs/CODE_REVIEW.md)
- [Development and deployment](docs/DEVELOPMENT.md)

## Solution structure

```text
LevelUp/
├── .github/
│   └── workflows/
├── docs/
├── scripts/
├── src/
│   ├── LevelUp.Domain/
│   ├── LevelUp.Application/
│   ├── LevelUp.Infrastructure/
│   └── LevelUp.Web/
├── tests/
│   ├── LevelUp.Domain.Tests/
│   ├── LevelUp.Application.Tests/
│   ├── LevelUp.Infrastructure.Tests/
│   └── LevelUp.Web.Tests/
├── Directory.Build.props
├── Directory.Packages.props
└── LevelUp.slnx
```

## Domain model

### User

Represents the account and owns identity, e-mail, password hash, language, theme and account lifecycle data.

### Character

Represents the gamified identity associated with a user. A user can have only one character, and character nicknames are globally unique in the current local data store.

### Habit

An independent recurring behavior. Habits do not belong to projects and can track positive, negative or bidirectional interactions.

### RecurringTask

An independent activity with daily, weekly or monthly recurrence. Tasks do not belong to projects.

### Project

Represents an objective. Its progress and completion are calculated from its To-Dos rather than maintained as duplicated mutable state.

### Todo

A project-scoped action. Every To-Do belongs to a project and inherits the current user as owner.

## Persistence

By default, runtime data is stored under:

```text
src/LevelUp.Web/Data/
```

The directory is intentionally ignored by Git except for `.gitkeep`. Never commit `LevelUpBD.json`, event journals or backup files because they may contain account information and password hashes.

Storage configuration is located in `src/LevelUp.Web/appsettings.json`:

```json
{
  "LevelUp": {
    "Storage": {
      "Directory": "Data",
      "FileName": "LevelUpBD.json",
      "BackupDirectory": "Backups",
      "BackupRetention": 10,
      "CreateBackupBeforeSave": true,
      "RecoverFromBackup": true,
      "WriteIndented": true
    }
  }
}
```

## Requirements

- .NET 10 SDK
- A supported browser
- Visual Studio 2026, Visual Studio Code or JetBrains Rider is optional
- IIS with the matching ASP.NET Core Hosting Bundle for Windows production hosting

## Run locally

```bash
git clone https://github.com/tiagoarrigoni/LevelUp.git
cd LevelUp
dotnet restore LevelUp.slnx
dotnet build LevelUp.slnx --configuration Debug
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Default development addresses:

```text
https://localhost:7245
http://localhost:5059
```

Main routes:

```text
/                 Entry and onboarding decision
/welcome          User onboarding
/character/create Character creation
/daily            Daily dashboard
/settings         User settings
```

## Tests

Run the complete test suite:

```bash
dotnet test LevelUp.slnx --configuration Release
```

The repository currently contains tests for domain behavior, application handlers and validators, JSON persistence, middleware and reusable Blazor design-system components.

## Health checks

```text
GET /health/live   Process liveness
GET /health/ready  Storage readiness
GET /health        Complete health report
```

## Continuous integration and deployment

- Pushes to `develop` and pull requests to `develop` or `main` run restore, build and tests.
- Pushes to `main` validate the solution and then publish/deploy through a Windows self-hosted runner.
- Production deployment uses `scripts/Deploy-LevelUp.ps1` and expects the IIS environment configured by the operator.

## Project status

Implemented foundation:

- Layered solution and centralized package versions
- Daily domain and UI
- JSON repository with recovery and backups
- User and character separation
- Character onboarding
- Design-system components
- Automated tests and health checks
- CI and IIS deployment workflow

Planned product work:

- Authentication sessions and logout flow
- Account editing for name, avatar, password, language and theme
- Complete interface standardization in English
- Statistics, achievements and notifications
- Optional relational or cloud persistence

## Security notes

This project is under active development. The local password hashing implementation is persistence-ready, but production authentication still requires a complete identity/session flow, secure secret management, HTTPS enforcement at the hosting boundary and an explicit account-recovery strategy.

Do not publish runtime JSON data, logs, backup directories, IIS secrets or runner credentials.

## Author

Developed by [Tiago Arrigoni](https://github.com/tiagoarrigoni).

## Authentication

LevelUp uses ASP.NET Core Cookie Authentication for persistent user sessions.

- `/login` authenticates registered users.
- `/auth/logout` clears the current session.
- `/daily` and `/account` require authentication.
- Credentials are verified in the Application layer through `IPasswordService`.
- The authenticated user identifier is stored as a `NameIdentifier` claim and synchronized with the current domain context at the start of each protected page.

### Sprint 2.3 — Onboarding

The authenticated onboarding flow is now Welcome → Account → Character → Class → Tutorial → Daily. Completion is persisted per user so the tutorial appears only on the first journey. Official typography uses Jersey 15 for brand/display text and Pixelify Sans for interface text.
