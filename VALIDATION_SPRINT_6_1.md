# Sprint 6.1 — Daily Card Layout Standardization

## Implemented

- Reduced the vertical distance between card titles and descriptions.
- Added a shared bottom-padding contract for Daily cards.
- Corrected the Habit balance footer so it no longer touches the card edge.
- Moved Task, To-Do, and Project metadata to the right, directly below the attribute badge.
- Standardized attribute badge typography, sizing, spacing, and alignment across all Daily cards.
- Applied Pixelify Sans to the Habits, Tasks, To-Dos, and Projects column headings.
- Preserved responsive behavior and empty optional states.

## Validation commands

```bash
dotnet format --verify-no-changes
dotnet build
dotnet test
git status
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
