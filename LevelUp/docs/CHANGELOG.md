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


## Unreleased — Phase 4 architecture hardening

### Added
- `LevelUp.Tests` with domain, progress, and persistence coverage.
- `QuestWorkflowService` and `QuestCompletionResult`.
- `IGameDataStore` and a GitHub Actions CI workflow.
- Explicit corrupted-save exception with backup path.

### Changed
- Quest-project association is encapsulated by the quest domain model.
- Archived quests and projects reject edits.
- `SaveService` accepts an injectable path and no longer writes UI messages.
- Quest completion orchestration was removed from `QuestScreen`.

## Unreleased — Phase 4 Milestone Foundation

### Added

- ordered Milestones with locked, created, active, completed, and archived states;
- optional quest-to-milestone association constrained to the same Project;
- Milestone rewards metadata and explicit one-time claiming;
- MilestoneService, BossService, and related workflow services;
- optional Boss Encounters linked to Milestones;
- automatic Milestone progression, Boss unlocking, and next-stage activation;
- Milestone persistence, cards, tables, screen, tests, and Event Storming documentation.

### Changed

- Project completion now respects valid Milestones when they exist;
- Project cards and tables display quest and milestone progress separately;
- completed or archived quests cannot change associations;
- deletion workflows preserve completed history.

## Phase 4 consolidation

- localized all player-facing interface text to Brazilian Portuguese;
- standardized Project, Chapter, Mission, Boss, Training, Character, and Finances terminology;
- preserved English source-code identifiers;
- separated recognition rewards from the future real-money Finances module;
- added centralized plain-text status localization;
- reviewed persistence-facing errors and Phase 4 product documentation.

## Unreleased — Phase 5 foundation

### Added
- Diary hub for Trainings, Missions, Projects, and Chapters.
- Backpack hub with Wallet.
- Wallet domain and service for real deposits and withdrawals.
- Required withdrawal justification, balance validation, transaction editing, deletion, history, and monthly summary.
- Library domain with planned, reading, completed, and archived books.
- Maximum of two simultaneous books in progress.
- Reading progress history with dates and XP per page.
- Persistence for books, reading history, and wallet transactions.
- Wallet, Book, and persistence tests.

### Changed
- Main navigation now exposes Character, Diary, Library, and Backpack.
- Removed the obsolete placeholder Gold screen.
- Clarified that Wallet is not an in-game economy.
