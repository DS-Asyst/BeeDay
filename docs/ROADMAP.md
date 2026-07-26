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

## Next product epic

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

## Later epics

- Attributes
- Library
- Statistics
- Achievements
- Product-wide accessibility, performance, and visual polish

Roadmap items are directional. A sprint must define invariants, affected layers, migration impact, and acceptance criteria before code changes begin.
