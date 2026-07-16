# LevelUp Architecture

## Overview

LevelUp is a C# and .NET console application with a feature-oriented domain and a Spectre.Console presentation layer.

```text
LevelUp/
├── Domain/
│   ├── Attributes/
│   ├── Character/
│   ├── Habits/
│   ├── Projects/
│   ├── Quests/
│   └── GameData.cs
├── Services/
│   ├── Character/
│   ├── Habits/
│   ├── Persistence/
│   ├── Projects/
│   └── Quests/
├── UI/
│   ├── Components/
│   │   ├── Character/
│   │   ├── Project/
│   │   ├── Quest/
│   │   ├── Shared/
│   │   └── Training/
│   ├── Infrastructure/
│   ├── Layout/
│   └── Screens/
├── Data/
└── docs/
```

## Layers

### Domain

Contains entities, state and business invariants. It does not depend on Spectre.Console, JSON files or screens.

### Services

Coordinates domain operations. Services are grouped by feature. `ProjectService` calculates progress and controls automatic completion; `QuestService` manages quest lifecycle and associations.

### Persistence

`SaveService` serializes and loads `GameData`. `GameStateService` is the only component responsible for creating a complete snapshot and requesting persistence.

### UI

- **Screens** coordinate navigation and user workflows.
- **Components** render cards and tables.
- **Infrastructure** centralizes input, messages, themes, icons and panel construction.
- **Layout** provides reusable low-level layouts such as `StatisticRow`.

## UI Foundation

`EntityCard` provides the common card structure used by feature-specific components. `StatisticRow` explicitly separates escaped plain text from trusted Spectre.Console markup.

```text
QuestCard / ProjectCard
          ↓
      EntityCard
          ↓
     StatisticRow
          ↓
      PanelBuilder
```

## Dependency Composition

Dependencies are composed manually in `Program.cs`. This keeps the current application simple while preserving clear constructor dependencies. A dependency-injection container may be introduced when additional application hosts are created.

## Persistence Flow

```text
Screen action
    ↓
Domain service mutation
    ↓
GameStateService.Save()
    ↓
SaveService.SaveGame()
    ↓
Data/save.json
```

## Future Interfaces

The domain and service layers are designed to be reused by future Blazor, API, desktop or mobile presentation layers.


## Phase 4 application workflows

Cross-feature use cases are coordinated by services under `Services/Workflows`. Screens collect input and render results; they do not decide how a completed quest affects its project or when state must be persisted.

Persistence is accessed through `IGameDataStore`. `SaveService` is the JSON implementation and accepts an explicit file path for automated tests. Infrastructure errors are surfaced to the UI instead of writing directly to the console.

The test project covers lifecycle transitions, project progress, archived-quest behavior, and JSON round trips.


## Phase 4 Workflows

- `QuestWorkflowService` recalculates Project and Milestone progress after Quest completion.
- `MilestoneWorkflowService` coordinates manual completion and safe deletion.
- `BossWorkflowService` completes the linked Milestone, activates the next stage, and attempts Project completion.
- `GameStateService` persists Projects, Quests, Milestones, Boss Encounters, Habits, and Character state as one snapshot.
