# Sprint 1.3 — Daily UX

## Scope

This sprint changes presentation behavior only. Domain rules, commands, persistence, task recurrence, To-Do ownership, and automatic Project completion remain unchanged.

## Implemented

- Independent vertical scrolling for Habits, Tasks, To-Dos, and Projects.
- Fixed module headers containing title, active counter, and a direct add button.
- Stable dashboard height based on the viewport; card growth no longer expands the Daily page.
- `Completed (N)` section in every module.
- Completed sections start collapsed and each module controls its own Show/Hide state.
- Completed Tasks and To-Dos can be reopened through their existing completion action.
- Completed Projects remain derived from their To-Dos and cannot be completed manually.
- Active and completed counters respect the current search filter.
- Project cards now use a non-interactive calculated-status indicator instead of an inert completion checkbox.

## Habit completion

Habits do not currently have a completion state in the domain. The Habits module still exposes the standardized `Completed (0)` section without inventing a new domain rule.

## Layout rule

The dashboard and each column use `min-height: 0`, constrained flex/grid sizing, and `overflow-y: auto`. Scrollbars appear only when content exceeds the available module height, which is approximately seven regular cards on the primary desktop viewport.
