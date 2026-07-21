# Component Library Validation

## Static checks performed

- A single web project remains under `src/LevelUp.Web`.
- Editor modal infrastructure is centralized in `EditorModalShell`.
- Standard editor and confirmation buttons use `LevelUpButton`.
- Text, textarea, select, checkbox, and date primitives exist under `Components/DesignSystem/Forms`.
- Habit, Task, To-Do, and Project editors use Design System form primitives.
- Empty dashboard columns use `LevelUpEmptyState`.
- No scoped CSS file contains `@import`.
- No Git metadata, `.vs`, `bin`, or `obj` directories are included in the delivery package.

## Local validation commands

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

## Environment limitation

The delivery environment does not contain the .NET SDK. Compilation and test execution must therefore be confirmed locally using the commands above.
