# LevelUp Web Frontend Architecture

## Guiding rule

The Blazor project is a presentation layer. Domain rules remain in the existing domain and application services.

## UI structure

- `Components/Layout`: application shell and top navigation.
- `Components/Dashboard`: dashboard-specific presentation components.
- `Components/Pages`: routable Blazor pages.
- `Shared`: generic design-system components.
- `ViewModels`: presentation models that prevent Razor components from owning business rules.

## Dashboard information architecture

The dashboard summarizes character progression and the four current work areas:

1. Habits
2. Tasks
3. To-Dos
4. Projects

Journal will become the operational area for creating and managing those entities in later sprints.
