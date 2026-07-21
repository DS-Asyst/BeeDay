# Delivery D — Theme & Tokens

## Objective

Consolidate the LevelUp visual language into semantic design tokens so components no longer depend on scattered literal values.

## Scope

- Brand, surface, content, border and status colors.
- Activity accent tokens for Tasks, To-Dos and Projects.
- Spacing, radius, elevation, focus, motion and z-index scales.
- Expanded typography scale and reusable type compositions.
- Global keyboard focus treatment.
- Reduced-motion behavior.
- Small layout and accessibility utility classes.
- Migration of repeated values in global and scoped component styles to semantic tokens.

## Files

- `wwwroot/css/variables.css`
- `wwwroot/css/typography.css`
- `wwwroot/css/theme.css`
- `wwwroot/css/utilities.css`
- `wwwroot/css/animations.css`
- Existing component and Design System styles migrated to tokens.

## Principles

1. Components consume semantic tokens rather than raw palette values.
2. The existing LevelUp identity remains unchanged.
3. Focus and motion behavior are consistent across components.
4. Future theme variants can override tokens without rewriting component CSS.

## Validation

Run:

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
