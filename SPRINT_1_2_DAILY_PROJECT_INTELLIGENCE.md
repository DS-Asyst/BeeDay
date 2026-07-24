# Sprint 1.2 — Daily Project Intelligence

Implemented on top of Sprint 1.1.

## Calculated Project information

- `TotalTodos`
- `PendingTodos`
- `CompletedTodos`
- `ProgressPercentage`
- `LastUpdatedAtUtc`
- `NextTodo`
- `Status`
- `Completed`

All calculated properties are marked with `JsonIgnore` and are not persisted.

## Automatic lifecycle

- Empty project: `Planned`, 0%.
- Todos exist but none are completed: `Planned`.
- Some completed and some pending: `InProgress`.
- At least one Todo and none pending: `Completed`.
- Adding or reopening a pending Todo automatically returns a completed project to `InProgress`.

## Manual completion removed

- Removed `ToggleProjectCommand`.
- Removed `ToggleProjectCommandHandler`.
- Removed `ToggleProjectAsync` from the web service.
- `Project.ToggleCompletion()` now rejects manual completion to protect the inherited Activity contract.
- No Project completion control exists in the current UI.

## Validation

Run locally:

```bash
dotnet clean
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
