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

**Status:** Planned

### Objective

Represent important project stages as RPG boss encounters.

### Planned Features

- Milestone entity;
- project milestone integration;
- milestone requirements and rewards;
- boss presentation components;
- milestone completion;
- final project boss;
- progress gates.

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
