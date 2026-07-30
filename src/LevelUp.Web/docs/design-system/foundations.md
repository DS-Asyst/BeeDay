# Foundations

## Stylesheet cascade

`Components/App.razor` loads foundation files before component files. Scoped Razor CSS is loaded near the end and may safely refine page layout. `typography-policy.css` must never be moved after `LevelUp.Web.styles.css`.

## Token ownership

- `css/variables.css`: colors, spacing, radius, elevation, focus, motion, controls and layers.
- `css/typography.css`: font families, scale and semantic type roles.
- `css/typography-policy.css`: low-specificity element defaults.
- `css/design-system.css`: canonical shared components.
- page `.razor.css`: composition and page-only layout.

## Rules

Use `var(--levelup-...)`. A literal value is acceptable only when it represents an intrinsic asset measurement or a documented one-off illustration detail. Never add a new global selector to correct one page.
