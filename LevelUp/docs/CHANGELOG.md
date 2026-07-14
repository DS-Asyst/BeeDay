# LevelUp Changelog

All notable project milestones are documented in this file.

The project currently follows development phases rather than strict semantic releases.

---

## Unreleased

### Architecture Track

Added:

- architecture documentation;
- official game terminology;
- roadmap documentation;
- decision log;
- contribution guidelines;
- project vision;
- changelog.

Changed:

- renamed `BossScreen` to `ProjectScreen`;
- aligned project terminology across UI and documentation;
- renamed `HabitsService.cs` to `HabitService.cs`;
- removed unused placeholder component files;
- clarified the distinction between domain terminology and RPG presentation.

---

## Phase 2 — Spectre.Console UI

### Added

- Spectre.Console dependency;
- navigable main menu;
- interactive training menu;
- reusable theme;
- reusable UI icons;
- PanelBuilder;
- ConsoleHelper improvements;
- InputReader prompts and validation;
- CharacterCard;
- AttributeTable;
- TrainingTable;
- TrainingCreatedCard;
- TrainingResultCard;
- ComingSoonCard;
- experience progress bar;
- confirmation prompts;
- interactive object selection.

### Changed

- replaced numeric menu navigation with SelectionPrompt;
- replaced manual training ID input with interactive selection;
- migrated character presentation to panels and tables;
- migrated training workflows to Spectre.Console components;
- organized UI into Screens, Components, Infrastructure and Layout;
- standardized visual feedback.

---

## Phase 1 — Core Foundation

### Added

- Character model;
- player attributes;
- attribute progression;
- character experience;
- level progression;
- Habit model;
- habit creation;
- habit completion;
- global and attribute-specific experience rewards;
- CharacterService;
- HabitService;
- AttributeService;
- ProgressionService;
- SaveService;
- JSON save and load;
- initial console menus;
- character creation;
- training workflows.

### Technical Foundation

- C# console application;
- .NET project structure;
- models and services separation;
- manual dependency composition in Program.cs;
- local JSON persistence.