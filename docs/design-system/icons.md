# Pixel Icon System

LevelUp uses one general-purpose icon renderer: `PixelIcon`. There is exactly one icon *system* in the application, backed by multiple icon *providers* hidden entirely behind the semantic contract.

## Core contracts

- `PixelIconName`: semantic icon identifier;
- `PixelIconSize`: supported size contract;
- `PixelIconColor`: semantic color contract;
- `PixelIconCategory`: logical grouping used by the registry and the catalog page;
- `PixelIconRegistry`: maps semantic names to sprite symbols and metadata;
- `PixelIcon`: renders the official SVG sprite reference.

Physical asset paths and provider identity must not leak into feature components. Feature code only ever references icons through `PixelIconName` — never a file path, never a provider-specific icon name (no raw Material Symbol name, no Devicon filename, no brand SVG path).

## Provider policy

LevelUp icons come from three providers, chosen per icon by what each one actually offers — never forced:

1. **Material Symbols** — primary provider for interface, actions, navigation, forms, feedback, and general application icons (including domain icons such as Profile, Level, Experience, Habit, Streak, etc. — Material's generic catalog covers these adequately; there is currently no icon that requires bespoke LevelUp artwork). Activity attributes (Strength, Dexterity, Intelligence, Vitality) are identified by name and a dedicated Design System color only and do not have icons.
2. **Devicon** — only for brands, technologies, and development tools that Devicon actually provides (verified against Devicon's own catalog, never assumed). Currently used for GitHub, Facebook, and LinkedIn.
3. **Official Brand** — current official brand artwork used by LevelUp for a brand Devicon does not provide. Preferred source is the brand owner's own official brand-resource or brand-guideline page; [Simple Icons](https://simpleicons.org/) may be used only when its path data accurately reproduces the current official mark — it is a curated source, never the brand's owner or issuer of the trademark. Currently used for YouTube, Instagram, and X (Devicon's only Twitter-family asset is the retired bird logo, which does not represent the current X brand and is not used as a substitute).
4. **LevelUp Custom** — reserved for genuinely product-specific domain icons where neither Material Symbols nor Devicon offers an acceptable semantic representation. Not currently used by any icon.

## Material Symbols configuration

One consistent configuration is used for every Material Symbols icon in LevelUp:

| Axis | Value |
| --- | --- |
| Family | Rounded |
| Fill | 1 (filled) |
| Weight | 400 |
| Grade | 0 |
| Optical size | 24 |

Rationale: LevelUp's UI is bold and solid (thick borders, saturated fills, rounded corners) — filled Rounded icons hold up at small sizes (12–16px) where thin outlined strokes would look weak, and match the app's visual weight better than Sharp or Outlined. Do not mix families/fill values across icons; if a future icon needs a different configuration, raise it as a deliberate design decision, not a one-off.

Assets are downloaded once as static SVGs from `symbols/web/{icon}/materialsymbolsrounded/{icon}_fill1_24px.svg` in the [google/material-design-icons](https://github.com/google/material-design-icons) repository — no variable font, no Google Fonts runtime, no CDN.

## Devicon and Official Brand boundaries

- Devicon is only used where it actually has the brand (verified against its `devicon.json` catalog, e.g. via the `original` SVG variant). Never invent a Devicon mapping for a brand it doesn't carry, and never use a Devicon technology/brand icon to represent a general UI action.
- Brand marks (Devicon and Official Brand alike) are **not** recolored to `currentColor` — their own authored colors (or intentionally colorless single-path marks) are preserved as-is. Only Material Symbols and LevelUp Custom icons are rewritten to `currentColor` so `PixelIconColor` works.
- If a brand LevelUp needs is unavailable in Devicon, prefer the brand owner's own official resource/guideline page; Simple Icons may be used only when it already accurately reflects the current official mark. Never substitute an unrelated logo, a fake Material Symbol impersonating the brand, or a retired/outdated mark — and never describe a community-maintained repository as the brand's owner or official source.

## Icon pipeline

```text
Provider source (design/icons/source/{material-symbols,devicon,official-brand,levelup-custom}/)
        │  semantic selection (design/icons/catalog/icon-mapping.csv)
        ▼
Application library (src/LevelUp.Web/wwwroot/icons/{provider-slug}/)
        │  scripts/New-IconSprite.ps1
        ▼
PixelIconRegistry (PixelIconName → symbol id, asset path, category)
        ▼
PixelIcon component (<svg><use href="/icons/sprite.svg#id"></use></svg>)
        ▼
Blazor UI
```

The application must never reference `design/icons/source/` directly — those folders are the immutable, unmodified source libraries. Only `src/LevelUp.Web/wwwroot/icons/` is production; it is a curated, re-exported subset of the sources, one file per `PixelIconName`, combined into a single shared `sprite.svg`.

## Source libraries

| Provider | Location | License | Notes |
| --- | --- | --- | --- |
| Material Symbols | `design/icons/source/material-symbols/` | Apache License 2.0 | https://fonts.google.com/icons |
| Devicon | `design/icons/source/devicon/` | MIT | https://devicon.dev/ — MIT notice must be preserved |
| Official Brand | `design/icons/source/official-brand/` | CC0 1.0 (markup only, current assets) | See the folder's `ATTRIBUTION.md` for the actual source and trademark owner per brand — brand marks remain trademarks of their owners regardless of asset source |
| Streamline Pixel (archived) | `design/icons/source/streamline-pixel/` | CC BY 4.0 | Retained as an archived reference pending a separately approved cleanup — no longer referenced by any production asset |

Each source folder has its own `ATTRIBUTION.md`. Never modify, rename, recolor, or delete files in any of these folders from application code or by hand — replace the row in `icon-mapping.csv` and regenerate instead.

## Application library

`src/LevelUp.Web/wwwroot/icons/` contains one standalone SVG per `PixelIconName`, organized by provider then by functional folder (a physical convenience; `PixelIconCategory` is the logical grouping used in code and the catalog page), plus the single shared `sprite.svg`:

```text
wwwroot/icons/
    material-symbols/
        actions/  navigation/  forms/  feedback/  activities/(habits/tasks/projects)
        attributes/  character/  books/  statistics/  system/
    devicon/
        social/
    official-brand/
        social/
    streamline/         (archived — unreferenced, pending approved cleanup)
    sprite.svg
```

Each individual file is a standalone `<svg viewBox="...">` (the provider's own native viewBox is preserved, not forced to a fixed value), with monochrome icons using `fill="currentColor"` and brand icons preserving their authored colors verbatim. `sprite.svg` combines all of them into `<symbol id="…">` elements for `<use>` referencing.

## Semantic mapping

`design/icons/catalog/icon-mapping.csv` is the single source of truth connecting a `PixelIconName` to a provider source file, with columns:

```text
PixelIconName, SymbolId, Provider, SourceName, Folder, Category, Variant, License
```

- `Provider` is one of `MaterialSymbols`, `Devicon`, `OfficialBrand`, `LevelUpCustom`.
- `SourceName` is the provider's own icon/brand identifier (e.g. `add`, `github-original`, `youtube`) — never a full file path.
- `Variant` records the exact configuration used (e.g. `Rounded-Fill1-Wght400-Grad0-Opsz24` for Material Symbols, `original` for Devicon).
- `License` records the license governing that specific source file.

A source SVG may be reused by more than one semantic name when they are genuinely the same concept (e.g. `Habit`, `Streak`, and `Repeat` all represent recurrence/consistency and share Material Symbols' `repeat` glyph — never `autorenew`, `loop`, `sync`, `refresh`, `local_fire_department`, or `trending_up`, which communicate the wrong concept for habits) — each still gets its own distinct `SymbolId`.

## How to add an icon

1. Confirm no existing `PixelIconName` already covers the need.
2. Pick a provider per the policy above (Material Symbols first, Devicon/Official Brand only for an actual brand, LevelUp Custom only if neither offers an acceptable representation).
3. Download the source SVG into the matching `design/icons/source/{provider}/` folder (never edit an existing source file in place).
4. Add the new value to `PixelIconName.cs`.
5. Add a row to `design/icons/catalog/icon-mapping.csv` (pick a unique `SymbolId`, `Folder`, `Category`, and record `Variant`/`License`).
6. Add the matching `Define(...)` line to `PixelIconRegistry.cs`, pointing at `/icons/{provider-slug}/{Folder}/{SymbolId}.svg`.
7. Run `scripts/New-IconSprite.ps1` to regenerate the application library and sprite.
8. Add test coverage following the existing pattern in `PixelIconTests.cs` / `IconMappingCsvTests`.
9. Run the mandatory validation commands (see `CLAUDE.md`).

## How to replace an icon's artwork

Change that row's `Provider`/`SourceName` (and `Variant`/`License` if the provider changed) in `design/icons/catalog/icon-mapping.csv`, download the new source file if needed, then re-run `scripts/New-IconSprite.ps1`. No change to `PixelIconName`, `PixelIconRegistry`, or any consumer is needed — the semantic contract (`PixelIconName.X`) is stable even when the underlying artwork or provider changes.

## How to add a brand

1. Verify the brand actually exists in Devicon (check its `devicon.json`/repository directly — never assume).
2. If present, download the `-original` (or most brand-accurate) variant into `design/icons/source/devicon/`.
3. If absent, source the current official artwork for `design/icons/source/official-brand/` — prefer the brand owner's own resource/guideline page, falling back to Simple Icons only when its path data already matches the current mark. Never substitute an unrelated or outdated logo; if no accurate source is available, report the gap rather than guessing.
4. Follow the same mapping/registry/generator steps as "How to add an icon", with `Provider` set to `Devicon` or `OfficialBrand` accordingly. Do not recolor the brand's artwork.

## The generator (`scripts/New-IconSprite.ps1`)

- Reads `icon-mapping.csv`, resolves each row's source file from its provider's folder.
- Preserves each source's own `viewBox` (falling back to `0 0 {width} {height}` if a source has no `viewBox`).
- Rewrites fills to `currentColor` only for monochrome providers (`MaterialSymbols`, `LevelUpCustom`); brand providers (`Devicon`, `OfficialBrand`) are left untouched.
- Rejects: unsafe/path-traversal tokens in `SymbolId`/`SourceName`/`Folder`, unknown providers, missing source files, duplicate `SymbolId`s, and SVGs it cannot parse.
- Is deterministic and idempotent — a pure function of the CSV and the source files, re-run any time either changes.
- Never writes to `design/icons/source/`.

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
- `.pixel-icon` sets `pointer-events: none` so icons never intercept clicks meant for their parent control.

## Licensing and trademark considerations

- Material Symbols: Apache License 2.0; attribution not required but the license notice is preserved in `ATTRIBUTION.md`.
- Devicon: MIT; the copyright/permission notice must be preserved (see `design/icons/source/devicon/LICENSE`), and depicted brand marks remain the trademarks of their respective owners independent of Devicon's own license.
- Official Brand: current assets are sourced via Simple Icons (CC0 1.0 for the SVG markup only — Simple Icons is a curated source, not a brand owner); YouTube, Instagram, and X (and their logos) remain trademarks of Google LLC, Meta Platforms, Inc., and X Corp. respectively — used only as plain links to LevelUp's own presence on each platform, not as an endorsement claim.
- Streamline Pixel: CC BY 4.0; attribution is retained in the archived source folder until that folder is separately approved for removal.

## Repository policy

- Do not embed general-purpose `<svg>` markup in feature Razor components.
- Do not reference `.svg` paths, provider icon names, or brand filenames directly from feature code.
- Do not reference any `design/icons/source/` folder from application code.
- Add semantic names and registry entries when extending the system — never a second icon system.
- Keep source assets, the mapping CSV, sprite symbols, registry entries, and tests synchronized. `scripts/New-IconSprite.ps1` and the `PixelIconContractTests`/`IconMappingCsvTests` exist specifically to catch drift between them.
