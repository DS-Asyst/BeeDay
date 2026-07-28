# Official Brand — Attribution

`OfficialBrand` means: the current official brand artwork used by LevelUp for a brand that Devicon does not provide. It does not mean any particular repository owns or issues the trademark — brand ownership always remains with the brand owner, regardless of where the SVG file was downloaded from.

Preferred sourcing order for a brand under this provider:

1. The brand owner's own official brand-resource or brand-guideline page.
2. [Simple Icons](https://simpleicons.org/), used only when its path data accurately reproduces the current official mark (verified by eye against the brand's own guidelines, not assumed).

## What this covers

Currently used: YouTube, Instagram, X — verified directly against Devicon's own `devicon.json` catalog on 2026-07-28 as not present there (Devicon's only Twitter-family asset is the retired bird logo, which does not represent the current X brand and is not used as a substitute).

| Brand | Actual asset source | License (markup) | Trademark owner |
| --- | --- | --- | --- |
| YouTube | Simple Icons (`youtube.svg`), matches the current YouTube play-button mark | CC0 1.0 Universal | Google LLC |
| Instagram | Simple Icons (`instagram.svg`), matches the current Instagram glyph | CC0 1.0 Universal | Meta Platforms, Inc. |
| X | Simple Icons (`x.svg`), matches the current X mark (not the retired Twitter bird) | CC0 1.0 Universal | X Corp. |

- **Simple Icons repository**: https://github.com/simple-icons/simple-icons
- **Markup license**: CC0 1.0 Universal (see `LICENSE.md` in this folder) — this covers only the SVG path data as authored/curated by the Simple Icons project.

## Trademark notice

The CC0 license applies only to the SVG markup files, not to the brand marks they depict. Simple Icons is a community-maintained project, not a brand owner, and never a substitute for one — it is used here strictly as a convenient, verified-accurate source of a mark that a brand owner has already made public. YouTube, Instagram, and X (and their logos) remain the trademarks of their respective owners regardless of this file's source. Each mark must be used in accordance with that owner's brand guidelines; this project uses them only as plain, unmodified links to LevelUp's own presence on each platform, not as an endorsement claim.

## Rules for this folder

- These are reference source files only. Never modify, rename, or recolor them beyond what each brand's own guidelines permit for single-color/monochrome usage.
- Never reference these files directly from application code — the application must only consume icons through `PixelIcon`/`PixelIconRegistry`, which point at `src/LevelUp.Web/wwwroot/icons/official-brand/`.
- If a brand's official mark changes (rebrand, redesign), prefer re-sourcing directly from the brand owner's current guidelines; fall back to Simple Icons only if it has already been updated to match. Replace the file here and re-run `scripts/New-IconSprite.ps1` — do not patch the generated copy.
- Record the actual source, license, and trademark owner for any new brand added under this provider, following the table above.
