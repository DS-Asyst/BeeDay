# LevelUp Decision Log

This document records decisions that affect the architecture, domain language or long-term direction of LevelUp.

---

# Era I — Foundation

## 2026-07 — Use Character as the user progression model

### Decision

Represent the user through a `Character` domain model.

### Context

LevelUp uses RPG progression mechanics to represent real-world productivity.

### Consequences

- experience and levels belong to the character;
- attributes belong to the character;
- future achievements, titles and gold may belong to the character;
- the UI can represent the user as an RPG character.

---

## 2026-07 — Centralize level progression

### Decision

Use `ProgressionService` to centralize level-up behavior.

### Context

Both the character and attributes require similar progression logic.

### Consequences

- shared progression rules remain consistent;
- models can implement `ILevelProgress`;
- level logic is not duplicated across services.

---

## 2026-07 — Use JSON persistence

### Decision

Persist game state locally using JSON.

### Context

The initial version requires simple local persistence without database infrastructure.

### Consequences

- save files remain human-readable;
- setup remains simple;
- SaveService handles serialization and compatibility;
- repository or database abstractions may be introduced later.

---

# Era II — Presentation

## 2026-07 — Adopt Spectre.Console

### Decision

Use Spectre.Console as the terminal presentation library.

### Context

The original UI relied on plain `Console.WriteLine` and numeric menus.

### Consequences

- menus support keyboard navigation;
- tables and panels improve readability;
- prompts provide built-in validation;
- the application remains a console program;
- Spectre.Console remains restricted to the UI layer.

---

## 2026-07 — Create reusable UI components

### Decision

Separate screens from visual components.

### Context

Character and training screens contained repeated layout and rendering logic.

### Consequences

- screens orchestrate workflows;
- components render domain information;
- UI infrastructure standardizes themes, icons and panels;
- components can be reused by future features.

---

## 2026-07 — Favor composition over screen inheritance

### Decision

Do not create a common base class for screens at the current stage.

### Context

The screens have different workflows and share limited behavior.

### Consequences

- screens remain explicit;
- components are composed as needed;
- rigid inheritance hierarchies are avoided;
- a shared abstraction may be introduced only if real repetition appears.

---

# Architecture Track

## 2026-07-14 — Rename BossScreen to ProjectScreen

### Decision

Rename `BossScreen` to `ProjectScreen`.

### Context

Projects represent real-world work.

Bosses are part of the RPG narrative and should not replace the project domain concept.

### Consequences

- the code uses `Project`;
- the main menu displays Projects;
- future project services and models will use real-world terminology;
- the UI may present project milestones as bosses.

---

## 2026-07-14 — Separate domain terminology from RPG terminology

### Decision

Use real-world terminology in the domain and RPG terminology in the presentation layer.

### Context

Terms such as Habit, Training, Project and Boss were being used inconsistently.

### Consequences

- `Habit` remains the domain name;
- the UI presents habits as Trainings;
- `Quest` remains the same in both layers;
- `Project` remains the domain and menu name;
- `Milestone` may be presented as Boss;
- terminology is documented in GameTerminology.md.

---

## 2026-07-14 — Treat architecture work as a continuous track

### Decision

Do not assign architecture and documentation improvements to a numbered product phase.

### Context

Product phases should represent user-facing capabilities.

Architecture, terminology and documentation support multiple phases.

### Consequences

- the roadmap contains an Architecture Track;
- product phases remain focused on delivered behavior;
- architecture work can continue alongside feature development.

---

## 2026-07-14 — Remove unused placeholder components

### Decision

Delete empty UI component files before their associated domains are implemented.

### Context

Empty files for Gold, Projects and Quests suggested completed architecture that did not yet exist.

### Consequences

- the repository contains only meaningful implementations;
- components will be created when real consumers exist;
- speculative abstractions are reduced.

---

## 2026-07-14 — Avoid premature Activity abstraction

### Decision

Do not introduce a shared `Activity` base type for Habit and Quest yet.

### Context

Habits and quests may eventually share reward or completion behavior, but quests are not implemented.

### Consequences

- Habit remains independent;
- Quest will be modeled based on real requirements;
- a shared abstraction will only be introduced after duplication becomes clear.

## 2026-07-14 — Organize domain and services by feature

### Decision

Replace the generic Models structure with a Domain structure organized by feature, and organize application services using the same domain boundaries.

### Context

The number of domain concepts was increasing with Projects, Quests, Titles, Achievements and Gold.

Keeping all models and services in flat folders would make navigation and ownership unclear.

### Consequences

- domain types are grouped by business concept;
- namespaces match the physical folder structure;
- services are grouped by the feature they coordinate;
- ProjectService belongs to Services/Projects;
- SaveService belongs to Services/Persistence;
- future Quest types and services will have dedicated folders;
- references throughout Program, UI and services use the new namespaces.
---

## 2026-07-15 — Centralize complete game-state persistence

### Decision

Use `GameStateService` as the single creator and persistence entry point for complete `GameData` snapshots.

### Context

Multiple screens previously assembled partial snapshots, creating a risk that newer feature data could be overwritten.

### Consequences

- screens request `gameStateService.Save()`;
- `SaveService` remains responsible for JSON serialization;
- adding a persisted feature requires changing the snapshot in one place;
- partial saves no longer remove unrelated data.

---

## 2026-07-15 — Derive project progress from non-archived quests

### Decision

Calculate project progress from linked quests whose status is not Archived.

### Context

Archived work should not block completion or reduce the meaningful progress of an active project.

### Consequences

- progress is never stored;
- archived quests are excluded from numerator and denominator;
- an active project completes automatically when all remaining valid quests are completed;
- projects with no valid quests remain at zero percent.

---

## 2026-07-15 — Introduce shared EntityCard foundation

### Decision

Use `EntityCard` and explicit text/markup statistic rows as the shared foundation for domain cards.

### Context

Quest and Project cards repeated panel, grid, label and escaping behavior.

### Consequences

- feature cards focus on mapping domain data;
- plain text is escaped exactly once;
- trusted Spectre.Console markup is explicit;
- future Achievement, Reward and Inventory cards can reuse the same structure.

---

## 2026-07-15 — Use entity-centered screen navigation

### Decision

Let users select a Project or Quest before choosing actions for that entity.

### Context

Operation-first CRUD menus become crowded as features grow and do not resemble the intended RPG board experience.

### Consequences

- top-level screens expose create, open, list and back;
- edit, complete, archive, reassignment and deletion are contextual actions;
- the interaction pattern can be reused by future modules.
