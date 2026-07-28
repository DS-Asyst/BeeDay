# Pixel Icon System

LevelUp uses one general-purpose icon renderer: `PixelIcon`. There is exactly one icon library in the application — every icon is derived from the Streamline Pixel collection.

## Core contracts

- `PixelIconName`: semantic icon identifier;
- `PixelIconSize`: supported size contract;
- `PixelIconColor`: semantic color contract;
- `PixelIconCategory`: logical grouping used by the registry and the catalog page;
- `PixelIconRegistry`: maps semantic names to sprite symbols and metadata;
- `PixelIcon`: renders the official SVG sprite reference.

Physical asset paths must not leak into feature components. Feature code only ever references icons through `PixelIconName` — never a file path, never a Streamline filename.

## Icon pipeline

```text
Streamline source (design/icons/source/streamline-pixel/)
        │  semantic selection (design/icons/catalog/icon-mapping.csv)
        ▼
Application library (src/LevelUp.Web/wwwroot/icons/streamline/)
        │  scripts/New-IconSprite.ps1
        ▼
PixelIconRegistry (PixelIconName → symbol id, asset path, category)
        ▼
PixelIcon component (<svg><use href="…/sprite.svg#id"></use></svg>)
        ▼
Blazor UI
```

The application must never reference `design/icons/source/streamline-pixel/` directly — that folder is the immutable, unmodified source library (662 SVGs downloaded from Iconify). Only `src/LevelUp.Web/wwwroot/icons/streamline/` is a production asset; it is a curated, re-exported subset of the source, one file per `PixelIconName`.

## Source library

- **Location**: `design/icons/source/streamline-pixel/`
- **License**: CC BY 4.0
- **Downloaded from**: https://icon-sets.iconify.design/streamline-pixel/
- Never modify, rename, recolor, or delete files in this folder. See `design/icons/source/streamline-pixel/ATTRIBUTION.md` for the full attribution notice.

## Application library

`src/LevelUp.Web/wwwroot/icons/streamline/` contains only icons actually used by the application — one standalone SVG per `PixelIconName`, plus the generated `sprite.svg`. Folder structure groups files by their functional area, not 1:1 with `PixelIconCategory` (a folder is a physical convenience; `PixelIconCategory` is the logical grouping used in code and the catalog page):

```text
wwwroot/icons/streamline/
    actions/       attributes/    books/       character/
    feedback/      forms/         habits/      inventory/
    navigation/    projects/      social/      statistics/
    system/        tasks/
    sprite.svg
```

Each individual file is a standalone `<svg viewBox="0 0 32 32">` with `fill="currentColor"` paths, sourced verbatim (path data unmodified) from the matching Streamline source file. `sprite.svg` combines all of them into `<symbol id="…">` elements for `<use>` referencing.

## Semantic mapping

`design/icons/catalog/icon-mapping.csv` is the single source of truth connecting a `PixelIconName` to a Streamline source file: columns `PixelIconName, SymbolId, Folder, Category, SourceFile`. Every semantic icon maps to exactly one source SVG; a source SVG may be reused by more than one semantic name when they are genuinely the same concept (e.g. `Check`, `Complete`, `Success`, and `CheckboxChecked` all currently reuse the same checkmark artwork under different semantic identities — each still gets its own distinct symbol id).

Where the Streamline collection has no direct visual equivalent for an existing LevelUp icon (it is an illustrative/thematic pack, not an abstract UI-glyph set — it has no bare plus, chevron, checkmark, or close/X glyph, and no GitHub mark), the mapping uses the closest available semantic match, prioritizing meaning over pixel-perfect visual similarity. Notable examples: `Add` uses a hierarchy/node icon containing an embedded plus; `ChevronLeft`/`ChevronRight` use circled navigation-arrow icons; `ChevronDown` uses an expand icon; `Cancel` uses a stop-sign icon; `More` uses a hamburger-menu icon; `GitHub` uses a generic code icon (no GitHub mark exists in the source collection).

## How to add an icon

1. Confirm no existing `PixelIconName` already covers the need.
2. Find a suitable source file in `design/icons/source/streamline-pixel/` (browse by filename — files are prefixed `streamline-pixel--{category}-{description}.svg`).
3. Add the new value to `PixelIconName.cs`.
4. Add a row to `design/icons/catalog/icon-mapping.csv` (pick a unique `SymbolId`, a `Folder`, and a `PixelIconCategory` value).
5. Add the matching `Define(...)` line to `PixelIconRegistry.cs`.
6. Run `scripts/New-IconSprite.ps1` to regenerate the application library and sprite.
7. Add test coverage following the existing pattern in `PixelIconTests.cs`.
8. Run the mandatory validation commands (see `CLAUDE.md`).

## How to replace an icon's artwork

Change that row's `SourceFile` in `design/icons/catalog/icon-mapping.csv` to a different file from the source library, then re-run `scripts/New-IconSprite.ps1`. No change to `PixelIconName`, `PixelIconRegistry`, or any consumer is needed — the semantic contract (`PixelIconName.X`) is stable even when the underlying artwork changes.

## The authenticated Development catalog

Available at:

```text
/design-system/icons
```

Renders every registry entry, grouped by `PixelIconCategory`, with its symbol id, label, and asset path — useful for confirming a mapping renders correctly before committing.

## Accessibility

- Decorative icons use `aria-hidden="true"` and are not focusable.
- Informative icons require a non-empty accessible label.
- Icons inside controls are usually decorative when the control already has visible text or an accessible name.
- An unknown enum value falls back to the Warning icon rather than producing broken markup.

## Repository policy

- Do not embed general-purpose `<svg>` markup in feature Razor components.
- Do not reference `.svg` paths directly from feature code.
- Do not reference `design/icons/source/streamline-pixel/` from application code.
- Add semantic names and registry entries when extending the system — never a second icon system.
- Keep source assets, the mapping CSV, sprite symbols, registry entries, and tests synchronized. `scripts/New-IconSprite.ps1` and the `PixelIconContractTests`/`EveryRegistryAssetExistsOnDisk`/`SpriteFileContainsExactlyOneSymbolPerRegistryEntry` tests exist specifically to catch drift between them.
