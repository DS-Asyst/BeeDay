# Phase 4 UX Consolidation

This sprint improves the existing LevelUp experience without introducing Phase 5 features.

## Decisions

- Source code, identifiers, namespaces, classes, and methods remain in English.
- All player-facing text remains in Brazilian Portuguese.
- `Project`, `Milestone`, `Quest`, `Boss`, `Training`, and `Gold` are displayed as Projeto, Capítulo, Missão, Chefe, Treinamento, and Carteira.
- Creation flows can be interrupted by typing `cancel` in any text or numeric prompt.
- Boolean decisions use explicit options: Sim, Não, and, during creation flows, Cancelar.
- Partial entities are never persisted when a creation flow is cancelled.
- Training navigation follows the same entity-centered pattern used by projects and quests.

## Training UX

The Training screen now supports:

- create;
- open;
- list;
- view details;
- edit;
- complete;
- delete;
- return.

## Technical corrections

- Removed duplicate quest deletion invocation.
- Removed duplicate project-association success call.
- Removed duplicate save after training completion.
- Habit IDs now continue correctly after loading persisted data.
- Training attribute names are displayed in Portuguese.

## Phase 5 boundary

No Phase 5 domain or feature was introduced in this sprint.
