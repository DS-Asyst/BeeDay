# LevelUp Roadmap

## Vision

LevelUp transforms real-world productivity into RPG character progression. Product phases deliver user-facing capabilities, while architecture improvements continue as a parallel track.

# Era I — Foundation

## Phase 1 — Core Foundation

**Status:** Completed

Delivered character creation, attributes, experience and level progression, habits, services, JSON persistence and the initial console interface.

# Era II — Presentation

## Phase 2 — Spectre.Console UI

**Status:** Completed

Delivered interactive navigation, centralized theme and icons, reusable panels, cards, tables, prompts, validation and training workflows.

# Era III — Gameplay

## Phase 3 — Projects and Quests

**Status:** Completed

### Delivered

- feature-oriented `Domain` and `Services` organization;
- `Project`, `ProjectStatus` and complete project lifecycle;
- `Quest`, `QuestStatus` and complete quest lifecycle;
- optional Quest-to-Project association;
- Project and Quest CRUD workflows;
- independent and project-linked quests;
- project progress calculated from non-archived quests;
- automatic project completion when all valid quests are completed;
- centralized persistence with `GameStateService`;
- contextual Project and Quest boards;
- `QuestCard`, `QuestTable`, `ProjectCard` and `ProjectTable`;
- shared `EntityCard` UI foundation;
- explicit plain-text and markup rendering in `StatisticRow`;
- updated architecture and product documentation.

## Phase 4 — Milestones and Boss Encounters

**Status:** In Progress

### Objective

Represent important project stages as ordered Milestones and optional RPG Boss Encounters.

### Delivered Foundation

- [x] Milestone entity and lifecycle;
- [x] ordered Project milestone integration;
- [x] optional Quest-to-Milestone association;
- [x] milestone requirements and reward metadata;
- [x] Boss Encounter domain and unlocking workflow;
- [x] milestone completion and sequential activation;
- [x] final project Boss workflow;
- [x] progress gates and persistence;
- [x] Milestone cards, tables, and screen;
- [x] automated domain, service, workflow, and persistence tests;
- [ ] reward delivery through the Gold module;
- [ ] richer Boss presentation and challenge templates.

# Era IV — Economy and Progression

## Phase 5 — Gold and Rewards

**Status:** Planned

- Gold balance;
- Reward model;
- reward configuration and summaries;
- transaction history;
- persistence;
- GoldScreen implementation.

## Phase 6 — Achievements and Titles

**Status:** Planned

- Achievement entity and conditions;
- Title entity and unlock rules;
- active title;
- character profile;
- progression milestones.

## Phase 7 — Advanced Character Progression

**Status:** Planned

- progression history;
- character statistics;
- rank or prestige systems;
- configurable progression balance.

# Era V — Insights and Engagement

## Phase 8 — Analytics

**Status:** Planned

- completion history;
- weekly and monthly summaries;
- streaks;
- attribute trends;
- project analytics;
- charts and exports.

## Phase 9 — Events

**Status:** Planned

- daily and weekly challenges;
- seasonal events;
- temporary quests;
- random encounters;
- event rewards.

# Era VI — Platform

## Phase 10 — Extensibility

**Status:** Future

- repository abstractions;
- API;
- Blazor web interface;
- cloud synchronization;
- multiple profiles;
- plugin architecture;
- desktop and mobile clients.

# Architecture Track

**Status:** Continuous

Current principles:

1. Domain rules are independent from presentation technology.
2. Screens coordinate workflows; components render information.
3. Persistence is centralized through `GameStateService`.
4. Abstractions are introduced only after concrete duplication appears.
5. Documentation changes with the code and terminology.


## Phase 4.0 — Architecture Hardening

- [x] Add automated domain, service, and persistence tests.
- [x] Extract quest-completion orchestration into a workflow service.
- [x] Strengthen archived-entity and project-association invariants.
- [x] Make JSON persistence path-injectable and presentation-agnostic.
- [x] Add a storage abstraction and continuous-integration workflow.
- [x] Add a UI error boundary for domain and storage failures.
- [x] Model milestones.
- [x] Model boss encounters.
