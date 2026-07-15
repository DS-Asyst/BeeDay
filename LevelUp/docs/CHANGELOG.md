# LevelUp Changelog

All notable project milestones are documented here.

## Unreleased

### Phase 3 — Projects and Quests

#### Added

- `Project`, `ProjectStatus` and full project lifecycle;
- `Quest`, `QuestStatus` and full quest lifecycle;
- `ProjectService` and `QuestService`;
- optional Quest-to-Project association;
- project progress calculated from completed, non-archived quests;
- automatic completion of active projects;
- Project and Quest persistence in `GameData`;
- centralized `GameStateService` snapshots;
- contextual Project and Quest boards;
- `ProjectCard`, `ProjectTable`, `QuestCard` and `QuestTable`;
- `EntityCard` shared UI foundation;
- status formatters for Projects and Quests.

#### Changed

- replaced the flat Models directory with feature-oriented Domain folders;
- organized Services by feature;
- changed Project and Quest screens from operation-first CRUD menus to entity-centered navigation;
- updated `StatisticRow` to distinguish escaped text from trusted markup;
- excluded archived quests from project progress;
- restricted new Quest associations to created or active projects;
- aligned README and documentation with the implemented architecture.

#### Fixed

- prevented duplicate escaping in cards;
- removed duplicate persistence calls in Quest project reassignment;
- corrected Quest menu syntax;
- preserved complete Projects and Quests when any screen saves the game.

## Phase 2 — Spectre.Console UI

Added interactive navigation, reusable themes and icons, panels, cards, tables, prompts, validation and training presentation.

## Phase 1 — Core Foundation

Added Character, attributes, progression, Habits, core services and local JSON persistence.
