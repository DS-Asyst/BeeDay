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
