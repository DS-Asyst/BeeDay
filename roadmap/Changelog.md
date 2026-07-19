# Changelog

## Sprint 3.1 — Dashboard Foundation

### Added

- Official LevelUp dashboard layout.
- Responsive top navigation with active-page state.
- Character progress header with Level and XP preview data.
- Action bar for Filters and Create.
- Dashboard columns for Habits, Tasks, To-Dos and Projects.
- Reusable dashboard column and empty-state components.
- Placeholder pages for Character, Journal, Inventory and Settings.
- Dashboard presentation ViewModel.

### Changed

- Replaced the design-system showcase on the home page.
- Updated the main layout to use the official application navigation.

### Preserved

- Current Git branch and complete `.git` directory.
- Existing domain, services, infrastructure and persistence behavior.

## Sprint 3.2 - Character Summary

### Added

- CharacterSummaryViewModel.
- Reusable CharacterSummaryCard component.
- Character level, experience progress and gold presentation.
- Responsive character summary layout.

### Changed

- DashboardHeader now delegates character presentation to CharacterSummaryCard.
- DashboardViewModel now composes CharacterSummaryViewModel.

### Notes

- Character values still use preview data.
- Domain service integration remains scheduled for the integration sprint.
