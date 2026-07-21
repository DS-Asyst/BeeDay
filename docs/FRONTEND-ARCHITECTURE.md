# Frontend Architecture

The Blazor frontend is located exclusively at `src/LevelUp.Web`.

## Layers

```text
Components/
├── DesignSystem/  reusable visual primitives
├── Features/      feature-oriented pages, components, models, and state
├── Layout/        application shell
├── Pages/         framework-level pages
└── Shared/        cross-feature components not yet promoted to the Design System
```

## Responsibilities

- **Pages** coordinate lifecycle and navigation.
- **State classes** hold UI state and orchestrate frontend operations.
- **Feature components** render feature-specific behavior.
- **Design System components** provide reusable visual and interaction contracts.
- **LevelUpWebService** remains the frontend boundary to Application use cases.

## State lifetime

Dashboard and Profile states are registered as scoped services, matching the Blazor Server circuit lifetime.

## Design System adoption

The activity editor modals share `EditorModalShell`, standard actions use `LevelUpButton`, and empty dashboard columns use `LevelUpEmptyState`. This removes repeated infrastructure markup while preserving feature-specific forms.
