# LevelUp

> Transform real-world productivity into RPG progression.

## Overview

LevelUp is a productivity RPG built with C# and .NET. It models real-world concepts such as habits, quests and projects in the domain layer, while the terminal UI presents them through RPG-inspired feedback using Spectre.Console.

## Current Features

- character creation and level progression;
- attribute progression;
- recurring habits presented as trainings;
- one-time quests, independent or linked to projects;
- project lifecycle and automatically calculated progress;
- automatic project completion when all active project quests are completed;
- contextual Project and Quest boards;
- reusable cards, tables, themes and UI infrastructure;
- centralized JSON persistence through `GameStateService`;
- feature-oriented Domain and Services organization.

## Architecture

```text
Presentation (Spectre.Console)
        ↓
Application Services
        ↓
Domain
        ↓
Persistence (JSON)
```

The domain remains independent from Spectre.Console so a future Blazor, API, desktop or mobile interface can reuse the same business rules.

## Project Structure

```text
LevelUp/
├── Domain/
│   ├── Attributes/
│   ├── Character/
│   ├── Habits/
│   ├── Projects/
│   ├── Quests/
│   └── Workflows/
├── Services/
│   ├── Character/
│   ├── Habits/
│   ├── Persistence/
│   ├── Projects/
│   ├── Quests/
│   └── Workflows/
├── UI/
│   ├── Components/
│   ├── Infrastructure/
│   ├── Layout/
│   └── Screens/
├── Data/
└── docs/
```

## Getting Started

```bash
git clone <repository-url>
cd LevelUp
dotnet restore
dotnet build
dotnet run --project LevelUp/LevelUp.csproj
```

For the best Unicode rendering, use Windows Terminal or another modern terminal with a Unicode-capable font.

## Quality Gates

```bash
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx
dotnet test LevelUp.Tests/LevelUp.Tests.csproj
```

The repository includes automated tests and a GitHub Actions workflow covering formatting, build, and test execution.

## Documentation

- `docs/Vision.md`
- `docs/Architecture.md`
- `docs/Domain.md`
- `docs/GameTerminology.md`
- `docs/Roadmap.md`
- `docs/DecisionLog.md`
- `docs/Contributing.md`
- `docs/CHANGELOG.md`

## Roadmap

Phase 3 — Projects and Quests is complete. The next product phase introduces milestones and boss encounters.

## License

MIT


## Phase 4 — Milestones and Boss Encounters

LevelUp now supports ordered Project Milestones, optional Quest-to-Milestone links, automatic stage progression, reward metadata, and optional Boss Encounters. See `LevelUp/docs/Phase4EventStorming.md` for the accepted domain rules.
