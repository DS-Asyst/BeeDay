# NES.css — Attribution

LevelUp uses a small, curated, LevelUp-recolored subset of NES.css as the implementation source for its internal pixel adapter (`../pixel-nes.css`). NES.css is **not** loaded globally and is **not** a general dependency of the application — see `docs/design-system/foundations.md` for the adapter's scope and restrictions.

- **Package**: `nes.css`
- **Source**: https://github.com/nostalgic-css/NES.css
- **Version**: 2.3.0
- **Tag**: `v2.3.0`
- **Commit**: `68d6d9e92403a0a78a0bf603d9cd4cbfdb636b8c`
- **Upstream source file**: `css/nes-core.css` (readable/unminified build — the minified build was intentionally not used as a source)
- **License**: MIT — Copyright (c) 2018 B.C.Rikko \<https://github.com/BcRikko\> (full text preserved in `nes-core.levelup-excerpt.css`)

## What this covers

`nes-core.levelup-excerpt.css` in this folder is a **provenance-only** reference copy of the exact upstream rule blocks LevelUp's pixel adapter was derived from. It is never linked from `App.razor` and is never loaded by the application — it exists solely so a future license/version audit can diff LevelUp's adapter against real upstream text.

## Included upstream selectors

- `.nes-container` (base) and `.nes-container.is-rounded` — the pixel-corner bordered-panel mechanic.
- `.nes-btn` (base, including `::after`, `:hover`, `:hover::after`, `:focus`, `:active:not(.is-disabled)::after`) and `.nes-btn.is-primary` (and its equivalent states) — the pixel-corner button mechanic.

## Intentionally excluded

The Bootstrap 4.1.3 Reboot/Normalize preamble; every bare-element selector (`html`, `body`, `a`, `button`, `input`, `select`, `textarea`, `table`, `th`, `td`, `label`, headings, lists, forms); the global `body,code,html,kbd,pre,samp{font-family:"Press Start 2P"}` rule; `.nes-container.is-dark` / `.is-centered` / `.is-right` / `.with-title`; `.nes-btn.is-success` / `.is-warning` / `.is-error` / `.is-disabled`; `.nes-btn`'s embedded `cursor: url(data:image/png;base64,...)` (LevelUp owns cursor styling globally via `cursors.css`); `.nes-btn input[type="file"]`; and every other NES.css component family (badge, dialog, balloon, icon, field, select, radio, checkbox, progress, list, and all other container/button variants). None of these have a real LevelUp consumer.

## How LevelUp's shipped adapter (`../pixel-nes.css`) differs from this excerpt

`pixel-nes.css` is the file the application actually loads. It does **not** contain any `.nes-*` class name — it defines exactly two LevelUp-owned classes, `.beeday-pixel-panel` and `.beeday-pixel-cta`, reproducing the border-image pixel-corner *mechanic* above but recolored:

- The container's pixel-corner fill is baked from `--beeday-game-ink` (`#171321`).
- The button's pixel-corner fill is baked from the Primary button variant's `--beeday-button-outline` (`#8d6500`).
- The upstream `.nes-btn`'s `::after` inset-shadow "3D press" depth layer, and its `:hover`/`:focus`/`:active` states, were evaluated but **not** adopted: `LevelUpButton` already implements an equivalent, working 3D-press mechanic via its own `box-shadow` system (see `design-system.css`), and layering NES's competing technique on top would produce a duplicated/conflicting depth effect. Only the pixel-corner border-image outline was adopted for the button; every other button state (hover, active, focus, disabled, loading) is unchanged, existing `LevelUpButton` behavior.
- `:focus` styling uses LevelUp's own `var(--beeday-focus-ring)`, never NES's own focus color.

Border-image `data:` URIs cannot reliably resolve `currentColor`/CSS custom properties across browsers, so the two fill colors above are baked into the SVG data URI text at authoring time rather than referenced live via `var()`. If `--beeday-game-ink` or the Primary variant's `--beeday-button-outline` value ever changes, the corresponding data URI in `pixel-nes.css` must be regenerated to match — this is a known, documented coupling, not an oversight.

## Restricted use

`.beeday-pixel-panel` and `.beeday-pixel-cta` are internal Design System adapter classes for a genuine, one-off special pixel experience — not general-purpose styling hooks. See `docs/design-system/foundations.md` for the required review process before applying either class to any new consumer.

## Rules for this folder

- `nes-core.levelup-excerpt.css` is a reference copy only. Do not link it from any page or component.
- Do not modify the upstream text captured in the excerpt beyond what is already documented above (the stripped `cursor` declaration) — if a future version bump changes what's needed, add a new dated excerpt rather than silently editing this one.
