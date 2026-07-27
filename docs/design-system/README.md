# Design System

The LevelUp Design System centralizes reusable Blazor UI primitives and the visual language used across feature modules.

## Documents

- [Visual foundations](foundations.md)
- [Components](components.md)
- [Typography](typography.md)
- [Pixel Icon System](icons.md)
- [UX and accessibility](ux-guidelines.md)

## Location

Reusable components live under:

```text
src/LevelUp.Web/Components/DesignSystem
```

Current component groups include Activities, Attributes, Buttons, Cards, Feedback, Forms, Icons, Layout, Modals, Pages, and Text.

## Rule

Feature code should consume shared contracts rather than reproduce local variants. A new shared component must solve a repeated, stable UI need; feature-specific behavior should remain in the owning feature.
