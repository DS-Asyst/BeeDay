# Typography

Typography must be applied by semantic role rather than by isolated per-component styling. Components must consume the semantic font tokens below — never reference a font family directly.

## Official font hierarchy

| Font | Semantic token | Purpose |
| --- | --- | --- |
| Inter | `--levelup-font-body` | Body text and readability: paragraphs, descriptions, forms, inputs, tables, dialogs, activity content, long text. |
| Press Start 2P | `--levelup-font-pixel` | The official pixel identity: XP, level, HUD, counters, badges, pixel labels, loading screens, achievements, and other pixel-specific UI elements. Must always remain loaded — do not remove it from the font `<link>` in `App.razor`. |
| Jersey 25 | `--levelup-font-ui` | Retro UI support: menus, navigation, secondary titles, panel headings, buttons, and other retro interface elements. Also the current fallback used for primary headings and brand/logo, since no separate "primary heading" role exists. |

```css
--levelup-font-body: "Inter", "Segoe UI", sans-serif;
--levelup-font-ui: "Jersey 25", "Segoe UI", sans-serif;
--levelup-font-pixel: "Press Start 2P", monospace;
```

`--levelup-font-family` remains as a legacy alias for `--levelup-font-ui`, kept only because a large number of chrome/panel components (navigation, side panels, modals) still reference it directly rather than the semantic token; new code should reference `--levelup-font-ui`, `--levelup-font-body`, or `--levelup-font-pixel` directly instead of the legacy alias.

## Removed fonts

- **Pixelify Sans** is not part of the Design System. It must never be loaded from Google Fonts. It was previously referenced (unloaded, silently falling back to Jersey) in three components; all three now consume `--levelup-font-ui` directly.
- **Jersey 15** is not part of the Design System. It must never be loaded from Google Fonts. Every former consumer (primary headings, brand/logo, large numeric displays) has been migrated to `--levelup-font-ui` or `--levelup-font-pixel` as appropriate; see the hierarchy table above.

## Roles

Typical roles include:

- application brand and major headings (`--levelup-font-ui`);
- page and section headings (`--levelup-font-ui`);
- card titles (`--levelup-font-ui`) and descriptions (`--levelup-font-body`);
- labels and field help (`--levelup-font-body`);
- metadata and status text (`--levelup-font-body`);
- button and navigation labels (`--levelup-font-ui`);
- validation, warning, and feedback messages (`--levelup-font-body`);
- XP, level, HUD, counters, badges, and other pixel-specific UI (`--levelup-font-pixel`).

## Rules

- Reuse shared text components or established classes.
- Consume the semantic font tokens (`--levelup-font-body` / `--levelup-font-ui` / `--levelup-font-pixel`); never hardcode a font family in a component.
- Preserve hierarchy through size, weight, spacing, and contrast.
- Do not use uppercase purely to compensate for weak hierarchy.
- Ensure small metadata remains legible and meets contrast requirements.
- Keep card title-to-description spacing consistent across activity types.
- Verify layout with long names, descriptions, translated text, and narrow viewports.
- Press Start 2P renders noticeably wider/blockier than Jersey or Inter at the same size; when applying `--levelup-font-pixel` to a fixed-width badge or counter, verify long values (multi-digit XP, high levels) do not overflow or clip.
