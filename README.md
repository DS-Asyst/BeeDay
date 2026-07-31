# LevelUp

LevelUp is a personal productivity application with RPG-inspired character progression. It is built with ASP.NET Core, Blazor Server, C#, and .NET 10.

The application combines daily activity management, projects, inventory, identity flows, and a character experience system while keeping domain rules independent from storage and presentation concerns.

## Current capabilities

- user registration, cookie authentication, email confirmation, and password recovery;
- account management, character creation, onboarding, and preferences;
- Daily management for Habits, recurring Tasks, To-Dos, and Projects;
- optional activity attributes: Strength, Dexterity, Intelligence, and Vitality;
- Inventory wallets, transactions, tags, filters, and responsive UI;
- experience curve, idempotent XP rewards, reward history, level calculation, and level-up feedback;
- JSON persistence with serialized access, atomic writes, backups, initialization, and recovery;
- reusable Blazor Design System and centralized Pixel Icon System;
- automated tests across Domain, Application, Infrastructure, and Web;
- GitHub Actions validation and controlled IIS deployment with health checks and rollback.

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

`LevelUp.Web` is the composition root. Detailed ownership and dependency rules are documented in [`docs/architecture/`](docs/architecture/README.md).

## Repository structure

```text
LevelUp/
├── CLAUDE.md
├── README.md
├── LICENSE
├── .editorconfig
├── .gitattributes
├── .gitignore
├── .github/
├── docs/
│   ├── architecture/
│   ├── development/
│   ├── design-system/
│   ├── domain/
│   └── ai/
├── scripts/
├── src/
├── tests/
├── Directory.Build.props
├── Directory.Packages.props
└── LevelUp.slnx
```

## Requirements

- .NET 10 SDK
- a supported modern browser
- PowerShell 7 for the Windows operational scripts
- Visual Studio 2022 or another editor with .NET 10 support

## Start locally

```bash
dotnet restore LevelUp.slnx
dotnet format LevelUp.slnx --verify-no-changes --no-restore
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Development data is written under `src/LevelUp.Web/Data`. Runtime data is ignored by Git except for the placeholder `.gitkeep`.

## Quality gate

Before opening or merging a pull request:

```bash
git status
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx --configuration Release --warnaserror
dotnet test LevelUp.slnx --configuration Release
```

## Branch strategy

- `hmg`: integration and validation
- `prd`: production
- temporary branches: `feature/*`, `fix/*`, `refactor/*`, `docs/*`, or `chore/*`

Changes should reach `prd` only after validation in `hmg`.

## Documentation

The documentation describes the current system, not the historical order in which features were implemented.

- [Documentation index](docs/README.md)
- [Architecture](docs/architecture/README.md)
- [Development](docs/development/README.md)
- [Design System](docs/design-system/README.md)
- [Domain](docs/domain/README.md)
- [AI collaboration contract](docs/ai/README.md)

Project-changing rules must live in the repository so ChatGPT, Claude Code, and human contributors use the same versioned source of truth.
