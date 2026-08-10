# BeeDay

BeeDay is a personal productivity application with light gamification and personal finance
tracking. It helps people organize habits, tasks, projects, and money in a single experience. It
is built with ASP.NET Core, Blazor Server, C#, and .NET 10.

The application combines daily activity management, projects, a personal Wallet, identity flows,
and a light XP/Level progression system, while keeping domain rules independent from storage and
presentation concerns. The gamification is intentionally light — inspired by products like
Duolingo rather than by RPG mechanics — and exists only to encourage consistency and visible
progress.

## Current capabilities

- user registration with full name, cookie authentication, email confirmation, and password
  recovery, hardened with login rate limiting and session invalidation on password change, reset,
  or deactivation;
- account management, profile creation, onboarding, and preferences;
- daily management for Habits, recurring Tasks, To-Dos, and Projects;
- optional productivity classifiers per activity (Strength, Dexterity, Intelligence, Vitality) for
  filtering, organization, and progress insights — not RPG character stats or distributable
  points;
- Wallet: transactions, tags, filters, and responsive UI for personal finance;
- experience curve, idempotent XP rewards, reward history, level calculation, and level-up
  feedback;
- SQL Server persistence via EF Core, with a single `BeeDayDbContext` and repositories scoped per
  Aggregate Root;
- reusable Blazor Design System and centralized Pixel Icon System;
- automated tests across Domain, Application, Infrastructure, Web, and end-to-end (Playwright);
- GitHub Actions validation and controlled IIS deployment with health checks and rollback.

## Architecture

```text
BeeDay.Domain
    ↑
BeeDay.Application
    ↑
BeeDay.Infrastructure
    ↑
BeeDay.Web
```

`BeeDay.Web` is the composition root. `BeeDay.Domain` has no dependency on ASP.NET Core or EF
Core. `BeeDay.Application` orchestrates use cases through 8 Aggregate-scoped repository
interfaces (`IUserRepository`, `IUserTokenRepository`, `IHabitRepository`,
`IRecurringTaskRepository`, `IProjectRepository`, `IWalletRepository`, `ITransactionRepository`,
`IWalletTagRepository`) plus `IUnitOfWork`, and never references `BeeDay.Infrastructure` directly.
`BeeDay.Infrastructure` provides the EF Core / SQL Server implementation. Detailed rules are
documented under [`docs/architecture/`](docs/architecture/).

## Persistence

SQL Server is the only persistence provider. There is no JSON storage, no legacy import, and no
compatibility layer — the database starts empty for every new environment. See
[ADR-002](docs/adr/ADR-002-greenfield-database.md),
[ADR-004](docs/adr/ADR-004-sql-server-runtime-cutover.md), and
[ADR-005](docs/adr/ADR-005-json-legacy-removal.md) for the decisions behind this.

## Repository structure

```text
BeeDay/
├── CLAUDE.md
├── README.md
├── LICENSE
├── .editorconfig
├── .gitattributes
├── .gitignore
├── .github/
├── docs/
├── scripts/
├── src/
│   ├── BeeDay.Domain/
│   ├── BeeDay.Application/
│   ├── BeeDay.Infrastructure/
│   └── BeeDay.Web/
├── tests/
│   ├── BeeDay.Domain.Tests/
│   ├── BeeDay.Application.Tests/
│   ├── BeeDay.Infrastructure.Tests/
│   ├── BeeDay.Web.Tests/
│   └── BeeDay.E2E.Tests/
├── Directory.Build.props
├── Directory.Packages.props
└── BeeDay.slnx
```

## Requirements

- .NET 10 SDK
- SQL Server (or SQL Server LocalDB for local development)
- a supported modern browser
- PowerShell 7 for the Windows operational scripts
- Visual Studio 2022 or another editor with .NET 10 support

## Start locally

```bash
dotnet restore BeeDay.slnx
dotnet format BeeDay.slnx --verify-no-changes --no-restore
dotnet build BeeDay.slnx
dotnet test BeeDay.slnx
dotnet run --project src/BeeDay.Web/BeeDay.Web.csproj
```

The connection string is read from configuration key `BeeDay:Persistence:SqlServer:ConnectionString`
(`SqlServerOptions.ConnectionString`) and is required — the application fails fast at startup
(`ValidateOnStart()`) if it is missing.

## Quality gate

Before opening or merging a pull request:

```bash
git status
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx --configuration Release --warnaserror
dotnet test BeeDay.slnx --configuration Release
dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure
```

## Tests

752 tests currently pass across five projects:

| Project | Tests |
|---|---|
| `BeeDay.Domain.Tests` | 93 |
| `BeeDay.Application.Tests` | 73 |
| `BeeDay.Infrastructure.Tests` | 129 |
| `BeeDay.Web.Tests` | 450 |
| `BeeDay.E2E.Tests` | 7 |
| **Total** | **752** |

All Infrastructure and Web integration tests run against a real, disposable SQL Server LocalDB
instance created per test run — never InMemory or SQLite. See
[`docs/testing/01-testing-strategy.md`](docs/testing/01-testing-strategy.md) for coverage,
infrastructure, and known limitations.

## E2E tests (Playwright)

`tests/BeeDay.E2E.Tests/` drives the real app through a real Chromium instance (a real Kestrel TCP
endpoint, not `TestServer`), with an isolated SQL Server database per test and no shared state
between tests.

Install the Chromium browser once per machine (after building the project, since Playwright's
install script ships in the build output):

```bash
dotnet build tests/BeeDay.E2E.Tests/BeeDay.E2E.Tests.csproj --configuration Release
pwsh tests/BeeDay.E2E.Tests/bin/Release/net10.0/playwright.ps1 install chromium
```

Run the suite:

```bash
dotnet test tests/BeeDay.E2E.Tests/BeeDay.E2E.Tests.csproj --configuration Release
```

On failure, a screenshot and a Playwright trace are written to
`tests/BeeDay.E2E.Tests/bin/Release/net10.0/e2e-artifacts/` (nothing is written on success).

## Branch strategy

- `hmg`: integration and homologation — default base for new Sprint/fix branches.
- `main`: consolidated version approved after homologation, promoted from `hmg` only.
- `prd`: production — promoted from `main` only.
- temporary branches: `sprint/*`, `feature/*`, `fix/*`, `refactor/*`, `docs/*`, or `chore/*`.

Changes flow `Sprint/fix branch → hmg → main → prd`. The
[`Validate Promotion`](.github/workflows/validate-promotion.yml) workflow enforces that pull
requests targeting `main` come from `hmg` and pull requests targeting `prd` come from `main`. See
also [`.github/workflows/ci.yml`](.github/workflows/ci.yml) and
[`.github/workflows/deploy-prd.yml`](.github/workflows/deploy-prd.yml).

## Documentation

The documentation describes the current system and the decisions behind it, not the historical
order in which features were implemented. Architecture Decision Records under
[`docs/adr/`](docs/adr/) are the one exception: they are immutable records of a decision at the
time it was made and are not updated when the system changes afterward.

- [Documentation index](docs/README.md)

Project-changing rules must live in the repository so ChatGPT, Claude Code, and human contributors
use the same versioned source of truth.

## License

Proprietary — see [`LICENSE`](LICENSE). No license is granted to copy, modify, distribute,
publish, sublicense, or use this software except with the copyright holder's prior written
permission.

<!-- CI/CD validation after default branch migration -->
