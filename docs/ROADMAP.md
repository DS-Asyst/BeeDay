# Roadmap

This roadmap reflects the current production baseline. Completed work is not repeated as pending work.

## Completed foundations

### Platform and productivity

- repository, persistence, security, CI, and deployment hardening;
- account registration, authentication, email confirmation, and password recovery;
- character onboarding and account management;
- Daily foundation for habits, recurring tasks, todos, and projects;
- Inventory domain, application layer, UI, filtering, responsive behavior, and tests;
- shared pixel-inspired UI design system.

### Epic 3 — RPG Core

Completed:

- character experience domain;
- linear experience curve;
- centralized and idempotent XP reward pipeline;
- experience transaction history and domain event;
- character-sidebar level and XP interface;
- loading and XP-gain feedback states;
- Domain, Application, Infrastructure, and Web test coverage.

Epic 3 is integrated into `prd`.

## Completed product epics

### Epic 4 — Character Progression

Purpose: define and implement the consequences of accumulated experience.

Planned scope:

- explicit level-up detection and processing;
- support for gaining multiple levels from one reward;
- level-up domain events;
- level-up rewards;
- level-up presentation and feedback;
- persistence and idempotency rules for granted progression rewards;
- tests across Domain, Application, and Web.

The business rules for rewards and activity XP should be reviewed before implementation so balancing decisions remain explicit.

### Epic 5 — Activity Attributes

Sprint 5.1 completed:

- optional semantic attribute for Habits, Tasks, To-Dos, and Projects;
- Strength, Dexterity, Intelligence, Wisdom, Vitality, and Charisma values;
- create and edit selection;
- JSON persistence and backward compatibility;
- domain and application validation;
- automated coverage;
- no integration with XP, levels, rewards, or character progression.

### Epic 6 — Pixel Icon System

Sprint 6.0 completed the architectural foundation:

- formally recorded the initial icon implementation as the migration baseline;
- defined the definitive migration to `PixelIcon` and `PixelIconName`, with no parallel general-purpose systems;
- inventoried current SVG assets, consumers, inline SVGs, direct references, and textual icon substitutes;
- limited Sprint 6.4 to icon integration with existing Forms and Dialogs;
- defined the registry, sprite, cache, naming, category, fallback, and accessibility targets;
- added an explicit SVG line-ending rule to `.gitattributes`.

Planned sequence:

1. Sprint 6.1 — Pixel Icon Infrastructure;
2. Sprint 6.2 — Navigation Icons;
3. Sprint 6.3 — Activity Icons;
4. Sprint 6.4 — Dialog & Forms Icon Integration;
5. Sprint 6.5 — Dashboard & Statistics Icons;
6. Sprint 6.6 — Final UI Polish.

## Later epics
- Library
- Statistics
- Achievements
- Product-wide accessibility, performance, and visual polish

Roadmap items are directional. A sprint must define invariants, affected layers, migration impact, and acceptance criteria before code changes begin.
