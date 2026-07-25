# Project Map

This document is the maintained technical inventory of the LevelUp source tree. It is generated from the reviewed source package and should be updated whenever projects, feature modules or cross-cutting services change.

## Snapshot

- Runtime source files: **289**
- C# type declarations: **271**
- Explicitly declared C# methods: **463**
- Razor components/pages: **58**
- CSS files: **33**
- JavaScript files: **2**
- Test source files: **33**
- `[Fact]` / `[Theory]` declarations: **138**

## Project responsibilities

| Project | Responsibility | Dependency rule |
|---|---|---|
| `LevelUp.Domain` | Entities, value objects, enums, domain events, invariants and domain exceptions. | Must not depend on another LevelUp project. |
| `LevelUp.Application` | Commands, queries, handlers, validators, contracts and orchestration. | Depends only on Domain and framework/application packages. |
| `LevelUp.Infrastructure` | JSON persistence, backups, security implementation, cache, journal, health checks and background processing. | Implements Application contracts; depends on Application. |
| `LevelUp.Web` | Blazor presentation, composition root, session/UI state, middleware and static assets. | May reference all runtime projects only to compose the application. |

## Domain inventory

### Entities

`Activity`, `Character`, `Habit`, `InventoryTag`, `LevelUpData`, `Project`, `RecurringTask`, `Todo`, `Transaction`, `User` and `Wallet`.

### Value objects

`ActivityDescription`, `ActivityTitle`, `CharacterNickname`, `EmailAddress`, `ProjectColor` and `UserName`.

### Domain events and exceptions

Domain events are based on `IDomainEvent` / `DomainEvent`; application actions are represented by `ApplicationActionDomainEvent`. Domain failures use dedicated domain exception types rather than presentation exceptions.

## Application feature map

### Authentication

5 source files covering commands/queries, handlers, requests/responses and validation as applicable.

### Characters

6 source files covering commands/queries, handlers, requests/responses and validation as applicable.

### Dashboard

3 source files covering commands/queries, handlers, requests/responses and validation as applicable.

### Habits

5 source files covering commands/queries, handlers, requests/responses and validation as applicable.

### Inventory

7 source files covering commands/queries, handlers, requests/responses and validation as applicable.

### Ordering

6 source files covering commands/queries, handlers, requests/responses and validation as applicable.

### Projects

5 source files covering commands/queries, handlers, requests/responses and validation as applicable.

### Tasks

5 source files covering commands/queries, handlers, requests/responses and validation as applicable.

### Todos

5 source files covering commands/queries, handlers, requests/responses and validation as applicable.

### Users

9 source files covering commands/queries, handlers, requests/responses and validation as applicable.

## Infrastructure map

- `Persistence/Json`: repository, serializer settings, atomic reader/writer, storage paths and backup/recovery.
- `Security`: PBKDF2 password hashing implementation.
- `Caching`: in-memory application cache.
- `Auditing`: newline-delimited JSON event journal.
- `Background`: bounded task queue and hosted worker.
- `HealthChecks`: storage readiness validation.
- `Configuration` and `DependencyInjection`: options and service registration.

## Web map

- `Components/DesignSystem`: reusable buttons, cards, forms, icons, feedback, layout, modal and text primitives.
- `Components/Features`: account, authentication, character creation, dashboard, habits, inventory, onboarding, projects, tasks and To-Dos.
- `Components/Layout`: shell, navigation, side panels, footer and reconnect UI.
- `Services` and `State`: browser-session/user context, UI services, toast handling and application façade.
- `Diagnostics` and `Middleware`: exception handling, correlation, security headers and request pipeline concerns.
- `wwwroot/css`: tokens, typography, theme, design-system rules, component foundations, animations and feature-level styles.

## Typography and styling

The official font family is loaded once in `Components/App.razor`:

- **Jersey 15** for display/brand typography.
- **Pixelify Sans** for UI text, labels, forms and controls.

Font usage is centralized through variables in `wwwroot/css/typography.css`. Feature styles should consume `--levelup-font-display`, `--levelup-font-brand` and `--levelup-font-family`; they should not declare external font names directly.

CSS ownership follows this rule: design tokens and global interaction behavior belong in shared CSS; page layout and feature-specific presentation remain in scoped component CSS or a dedicated feature stylesheet.

## Largest maintenance hotspots

The following areas deserve extra review because they coordinate multiple concerns:

- `LevelUpData`: aggregate ownership, validation, migrations and collection behavior.
- Dashboard `Home` and `DashboardState`: orchestration and UI state.
- Inventory page and inventory handlers: wallet, tags, filtering and transaction workflows.
- `JsonLevelUpRepository`: concurrency, recovery and persistence boundaries.
- `Program.cs`: application composition and HTTP pipeline.

These are not defects by themselves. Split them only when a cohesive responsibility can be extracted with tests.

## Text and localization policy

User-facing interface text is currently English. New UI strings should not be embedded repeatedly across components when localization begins; introduce resource files and a localization abstraction as one coordinated change rather than partial string extraction. Domain validation messages should remain stable and presentation-neutral.

## Removed redundancy

The duplicate, unused `ProfileSummaryCard` component was removed in favor of `CharacterSummaryCard`. Sprint-specific UI notes and the obsolete frontend-only README were also removed; durable guidance now lives in the root README and maintained documents under `docs/`.

## Validation

Run the following after every structural change:

```bash
dotnet restore LevelUp.slnx
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx --configuration Release
dotnet test LevelUp.slnx --configuration Release
```
