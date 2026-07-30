# Typography

Typography must be applied by semantic role rather than by isolated per-component styling. Components must consume the semantic font tokens below — never reference a font family directly.

The Design System officially uses two fonts only: Inter and Jersey 25.

## Official font hierarchy

| Font | Semantic token | Purpose |
| --- | --- | --- |
| Inter | `--levelup-font-body` | The default for the entire application: body text, descriptions, labels, placeholders, menus, dialogs, tables, forms, statistics, values, counters, balances, dates, badges, filters, cards, attributes, messages, helper text, HUD information, and every other regular UI element. |
| Jersey 25 | `--levelup-font-ui` | Reserved for: page titles, card titles, stylized buttons (the shared `LevelUpButton` component), and LevelUp branding only. Not used for menus, navigation, tabs, or general interface chrome. |

```css
--levelup-font-body: "Inter", "Segoe UI", sans-serif;
--levelup-font-ui: "Jersey 25", "Segoe UI", sans-serif;
```

`--levelup-font-family` remains as a legacy alias for `--levelup-font-ui`, kept only because a number of chrome/panel components (navigation brand, side panels, modals) still reference it directly rather than the semantic token; new code should reference `--levelup-font-ui` or `--levelup-font-body` directly instead of the legacy alias.

## Removed fonts

- **Press Start 2P** is not part of the Design System. It must never be loaded from Google Fonts. It was previously the pixel identity for XP/level/HUD/counter/balance displays (`--levelup-font-pixel`); every former consumer has been migrated to `--levelup-font-body` and the token has been removed.
- **Pixelify Sans** is not part of the Design System. It must never be loaded from Google Fonts. It was previously referenced (unloaded, silently falling back to Jersey) in three components; all three now consume `--levelup-font-ui` directly.
- **Jersey 15** is not part of the Design System. It must never be loaded from Google Fonts. Every former consumer (primary headings, brand/logo, large numeric displays) has been migrated to `--levelup-font-ui` or `--levelup-font-body` as appropriate.

## Roles

Typical roles include:

- application brand and major headings (`--levelup-font-ui`);
- page and section headings (`--levelup-font-ui`);
- card titles (`--levelup-font-ui`) and descriptions (`--levelup-font-body`);
- labels and field help (`--levelup-font-body`);
- metadata and status text (`--levelup-font-body`);
- the shared `LevelUpButton` component's label (`--levelup-font-ui`) — every other interactive control (bare buttons, menu items, tabs, options, navigation links) reads as regular UI text (`--levelup-font-body`);
- validation, warning, and feedback messages (`--levelup-font-body`);
- XP, level, HUD, counters, badges, balances, and other numeric/statistic displays (`--levelup-font-body`).

## Rules

- Reuse shared text components or established classes.
- Consume the semantic font tokens (`--levelup-font-body` / `--levelup-font-ui`); never hardcode a font family in a component.
- Preserve hierarchy through size, weight, spacing, and contrast.
- Do not use uppercase purely to compensate for weak hierarchy.
- Uppercase is prohibited on the shared button component (`LevelUpButton` always renders the exact text it is given); other UI elements (badges, labels, chips, tabs, navigation) may still use uppercase where it is an intentional, established design choice.
- Ensure small metadata remains legible and meets contrast requirements.
- Keep card title-to-description spacing consistent across activity types.
- Verify layout with long names, descriptions, translated text, and narrow viewports.
