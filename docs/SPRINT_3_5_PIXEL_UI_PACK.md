# Sprint 3.5 — Pixel UI Pack

## Objective

Consolidate LevelUp's pixel-art identity on top of the Sprint 3.4 design-system foundation without changing domain rules, persistence, routes, or application behavior.

## Delivered

- Semantic button states: normal, hover, pressed, disabled, loading, and focus-visible.
- Optional pixel icon support in `LevelUpButton` through the `Icon` parameter.
- Reusable `LevelUpIcon` component with a curated, dependency-free icon set.
- Pixel scrollbar styling for Chromium/WebKit plus Firefox fallback.
- Shared hover, press, pop, panel-enter, success, and error microinteraction utilities.
- Centralized timing, easing, outline, and pixel-shadow tokens.
- Consistent focus-visible treatment for buttons, links, form controls, interactive cards, and role-based controls.
- Reduced-motion and forced-colors fallbacks.

## Icon usage

```razor
<LevelUpButton Icon="LevelUpIconName.Save">SAVE</LevelUpButton>
<LevelUpIcon Name="LevelUpIconName.Inventory" Size="24" />
```

Available icons include Add, Edit, Delete, Save, Close, Search, Settings, User, Lock, Language, Check, navigation chevrons, More, Warning, Info, Inventory, Book, and Daily.

## Microinteraction utilities

- `levelup-pixel-hover`
- `levelup-pixel-press`
- `levelup-pixel-pop`
- `levelup-panel-enter`
- `levelup-success-pulse`
- `levelup-shake`

These classes are optional and can be added incrementally to feature components.

## Out of scope

- Custom cursors.
- Domain or persistence changes.
- Inventory and Library functionality.
- Structural redesign of existing pages.
- Third-party icon libraries.

## Validation

Run:

```bash
dotnet clean
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
