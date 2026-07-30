# Buttons

`LevelUpButton` is the canonical text-action component.

## Public parameters

- `Variant`: semantic color and action intent.
- `Typography`: `Ui`, `Body`, or `Pixel`.
- `Compact`, `FullWidth`, `Disabled`, `IsLoading`.
- `Icon` and `IconSize`.
- `Type`, `OnClick`, `Class` and unmatched HTML attributes.

## Variants

`Primary`, `Secondary`, `Success`, `Warning`, `Back`, `Danger`, `ConfirmationDanger`, `ConfirmationCancel`.

## Typography selection

```razor
<LevelUpButton Typography="LevelUpButtonTypography.Ui">Save</LevelUpButton>
<LevelUpButton Typography="LevelUpButtonTypography.Body">Learn more</LevelUpButton>
<LevelUpButton Typography="LevelUpButtonTypography.Pixel">Sign in</LevelUpButton>
```

Pixel typography is appropriate only for short labels. Do not use it for translated or dynamic labels without checking overflow.

Native `<button>` remains valid for icon-only controls, composite widgets, menu internals, checkboxes represented as buttons, and framework reconnect controls. Those controls must still provide accessible names and Design System focus behavior.
