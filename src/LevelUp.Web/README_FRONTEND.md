# LevelUp.Web

Blazor Server application and presentation layer for LevelUp.

## Organization

The frontend is organized by feature under `Components/Features`:

- `Dashboard`: page, components and dashboard state.
- `Habits`, `Tasks`, `Todos`, `Projects`: editors and form models.
- `CharacterCreation`: page, model and state for character creation.
- `Common`: types shared by the presentation layer.
- `Layout`: global application structure.
- `Shared`: reusable components across features.

## Estado

- `DashboardState`: dashboard data and operations.
- `DashboardModalState`: editors and deletion flows.
- `CharacterCreationState`: character creation flow.

The main state containers are registered as `Scoped` in `Program.cs`.

## Integration

The interface accesses the Application layer only through `LevelUpWebService`. JSON persistence is the exclusive responsibility of `LevelUp.Infrastructure`.

## Running the application

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

## Character experience panel

Character progression is displayed in the existing left-side character panel. The panel shows the current level, current-level XP, XP required for the next level, and a responsive progress bar. Activity counters are intentionally excluded because they belong to the Daily context rather than character progression.

XP feedback is emitted only when the persisted character total increases after an idempotent reward operation. Reopening and completing the same source again does not replay the feedback when no new reward is granted. Motion is implemented with scoped CSS and disabled when `prefers-reduced-motion` is enabled.
