# LevelUp

> `LevelUp` is a temporary product name.

LevelUp is a personal productivity application with light gamification and personal finance tracking. It helps people organize habits, tasks, projects, and money in a single experience. It is built with ASP.NET Core, Blazor Server, C#, and .NET 10.

The application combines daily activity management, projects, a personal Wallet, identity flows, and a light XP/Level progression system, while keeping domain rules independent from storage and presentation concerns. The gamification is intentionally light — inspired by products like Duolingo rather than by RPG mechanics — and exists only to encourage consistency and visible progress.

## Current capabilities

- user registration with full name, cookie authentication, email confirmation, and password recovery, hardened with login rate limiting and session invalidation on password change, reset, or deactivation;
- account management, profile creation, onboarding, and preferences;
- Daily management for Habits, recurring Tasks, To-Dos, and Projects;
- optional productivity classifiers per activity (Strength, Dexterity, Intelligence, Vitality) for filtering, organization, and progress insights — not RPG character stats or distributable points;
- Wallet: transactions, tags, filters, and responsive UI for personal finance;
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

`LevelUp.Web` is the composition root. Detailed ownership and dependency rules are documented in [`docs/architecture/`](docs/architecture/01-current-state.md).

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
│   ├── adr/
│   ├── architecture/
│   ├── contracts/
│   ├── data/
│   ├── openapi/
│   ├── operations/
│   ├── security/
│   └── testing/
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

## Web integration tests

`tests/LevelUp.Web.Tests/Integration/` runs the app for real (`WebApplicationFactory<Program>` /
TestServer): real cookies, real antiforgery, real `OnValidatePrincipal`, isolated JSON storage per
test run. Run just these:

```bash
dotnet test tests/LevelUp.Web.Tests/LevelUp.Web.Tests.csproj --configuration Release --filter "FullyQualifiedName~Integration"
```

See `docs/testing/01-testing-strategy.md` section 6 for coverage, infrastructure, and known
limitations (e.g. HSTS can't be exercised through TestServer, which never performs a real TLS
handshake).

## E2E tests (Playwright)

`tests/LevelUp.E2E.Tests/` drives the real app through a real Chromium instance: a real Kestrel
TCP endpoint (not TestServer), isolated JSON storage per test, no shared state between tests.
Covers 7 user journeys — account creation, login/onboarding, logout, profile editing, habit
creation/completion, task creation/completion, and Wallet tag/transaction/balance — and nothing
already covered by the Web integration tests (antiforgery, cookies, SessionVersion, rate limiting).

Install the Chromium browser once per machine (after building the project, since Playwright's
install script ships in the build output):

```bash
dotnet build tests/LevelUp.E2E.Tests/LevelUp.E2E.Tests.csproj --configuration Release
pwsh tests/LevelUp.E2E.Tests/bin/Release/net10.0/playwright.ps1 install chromium
```

Run the suite:

```bash
dotnet test tests/LevelUp.E2E.Tests/LevelUp.E2E.Tests.csproj --configuration Release
```

On failure, a screenshot and a Playwright trace are written to
`tests/LevelUp.E2E.Tests/bin/Release/net10.0/e2e-artifacts/` (nothing is written on success). View a
trace with:

```bash
pwsh tests/LevelUp.E2E.Tests/bin/Release/net10.0/playwright.ps1 show-trace tests/LevelUp.E2E.Tests/bin/Release/net10.0/e2e-artifacts/<test-name>.trace.zip
```

See `docs/testing/01-testing-strategy.md` section 7 for infrastructure details and selector
conventions.

## Branch strategy

- `hmg`: integration and validation
- `prd`: production
- temporary branches: `feature/*`, `fix/*`, `refactor/*`, `docs/*`, or `chore/*`

Changes should reach `prd` only after validation in `hmg`.

## Documentation

The documentation describes the current system and the approved target architecture, not the historical order in which features were implemented.

- [Documentation index](docs/README.md)

Project-changing rules must live in the repository so ChatGPT, Claude Code, and human contributors use the same versioned source of truth.
