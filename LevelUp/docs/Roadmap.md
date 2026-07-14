# LevelUp Roadmap

## Vision

LevelUp transforms real-world productivity into RPG character progression.

The roadmap is divided into product eras and phases.

Architecture work is treated as a continuous track rather than a user-facing product phase.

---

# Era I — Foundation

## Phase 1 — Core Foundation

**Status:** Completed

### Objective

Build the initial domain, progression system and persistence layer.

### Delivered

- Character model
- Character creation
- Attribute system
- Attribute progression
- Experience system
- Level progression
- Habit model
- Habit creation
- Habit completion
- CharacterService
- HabitService
- AttributeService
- ProgressionService
- SaveService
- JSON persistence
- Initial main menu
- Basic console interface

---

# Era II — Presentation

## Phase 2 — Spectre.Console UI

**Status:** Completed

### Objective

Transform the plain console interface into an organized RPG-inspired terminal experience.

### Delivered

#### Infrastructure

- Spectre.Console integration
- centralized visual theme
- centralized UI icons
- ConsoleHelper
- InputReader
- PanelBuilder
- reusable statistic rows

#### Navigation

- interactive main menu
- interactive training menu
- selection prompts
- confirmation prompts
- validation feedback

#### Character UI

- CharacterCard
- AttributeTable
- experience progress bar
- character details screen

#### Training UI

- TrainingTable
- TrainingCreatedCard
- TrainingResultCard
- interactive attribute selection
- interactive habit selection
- training completion confirmation

#### Shared UI

- ComingSoonCard
- consistent placeholder screens
- standardized panels and messages

---

# Architecture Track

**Status:** In Progress

## Objective

Prepare the codebase for long-term domain evolution.

## Deliverables

- Vision.md
- Architecture.md
- GameTerminology.md
- Roadmap.md
- DecisionLog.md
- Contributing.md
- Changelog.md
- naming consistency
- namespace consistency
- repository cleanup
- README revision
- domain review

## Current Decisions

- the domain uses real-world terminology;
- the UI may use RPG metaphors;
- `Habit` is presented as Training;
- `Project` replaces the previous project-level use of Boss;
- `Milestone` may be presented as Boss;
- presentation infrastructure remains independent of domain rules.

---

# Era III — Gameplay

## Phase 3 — Projects

### Phase 3 progress

- [x] Project domain model
- [x] Project lifecycle
- [x] ProjectStatus
- [x] ProjectService foundation
- [x] Project persistence
- [x] Domain organization by feature
- [x] Service organization by feature
- [ ] Quest domain
- [ ] Project progress calculation
- [ ] Project workflows
- [ ] Project UI

### Objective

Implement long-term objectives composed of multiple tasks.

### Planned Features

- Project entity
- ProjectService
- project creation
- project editing
- project status
- project progress
- project completion
- active and archived projects
- project rewards
- project persistence
- ProjectScreen
- ProjectTable
- ProjectCard

## Phase 4 — Quests

**Status:** Planned

### Objective

Implement one-time tasks and integrate them with projects.

### Planned Features

- Quest entity
- QuestService
- quest creation
- quest editing
- quest completion
- quest status
- standalone quests
- project-linked quests
- quest rewards
- due dates where appropriate
- quest persistence
- QuestScreen
- QuestTable
- QuestDetailsCard

## Phase 5 — Milestones and Boss Encounters

**Status:** Planned

### Objective

Represent important project stages as RPG boss encounters.

### Planned Features

- Milestone entity
- project milestone integration
- milestone requirements
- milestone rewards
- boss presentation components
- milestone completion
- final project boss
- progress gates

---

# Era IV — Economy and Progression

## Phase 6 — Gold and Rewards

**Status:** Planned

### Objective

Introduce a flexible reward and economy system.

### Planned Features

- Gold balance
- Reward model
- gold rewards
- reward summaries
- transaction history
- reward configuration
- persistence
- GoldScreen
- GoldSummary

## Phase 7 — Achievements and Titles

**Status:** Planned

### Objective

Expand character identity and long-term progression.

### Planned Features

- Achievement entity
- achievement conditions
- unlocked achievements
- Title entity
- title unlock rules
- active title
- character profile
- progression milestones

## Phase 8 — Advanced Character Progression

**Status:** Planned

### Objective

Provide deeper RPG progression beyond basic levels.

### Planned Features

- character statistics
- level history
- attribute history
- prestige or rank system
- progression summaries
- configurable progression balance

---

# Era V — Insights and Engagement

## Phase 9 — Analytics

**Status:** Planned

### Objective

Turn activity history into useful productivity insights.

### Planned Features

- completion history
- weekly summaries
- monthly summaries
- streaks
- attribute trends
- project analytics
- productivity charts
- exportable reports

## Phase 10 — Events

**Status:** Planned

### Objective

Introduce dynamic and time-based gameplay.

### Planned Features

- daily challenges
- weekly challenges
- seasonal events
- temporary quests
- random encounters
- event rewards

---

# Era VI — Platform

## Phase 11 — Extensibility

**Status:** Future

### Objective

Prepare LevelUp for multiple integrations and presentation layers.

### Possible Features

- import and export
- repository abstractions
- API
- cloud synchronization
- multiple profiles
- plugin architecture
- desktop interface
- web interface
- mobile interface

---

# Roadmap Principles

1. Domain rules are implemented before complex UI components.
2. UI components are created only for implemented domain behavior.
3. Architecture abstractions are introduced only when justified by real complexity.
4. Each phase should deliver a usable and testable increment.
5. Documentation must be updated when terminology or architecture changes.