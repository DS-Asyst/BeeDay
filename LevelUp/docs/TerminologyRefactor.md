# Terminology refactor

The executable Console application now uses the schema 7 vocabulary:

- Habit replaces Training.
- Task is an independent recurring item.
- To-do belongs to a Project and may later be associated with a Milestone.
- Project and Milestone keep their names.

The active composition root, session snapshot, journal navigation, habit screen, task screen, and project/to-do screen use these terms in English. No Blazor project or component is included.

The old Quest implementation remains only as disconnected source compatibility for the earlier workflow tests. It is no longer composed by `ApplicationBootstrap`, no longer stored by `GameStateService`, and is not reachable from the Console menu. It should be deleted after the old workflow tests are rewritten against `TaskService` and `ProjectTodoService`.
