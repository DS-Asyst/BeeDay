# Pixel Icon Library

The Pixel Icon Library by HackerNoon is the current source of the LevelUp pixel SVG assets.

This document records the existing Sprint 5 implementation. The definitive Epic 6 architecture, migration plan, inventory, accessibility policy, and sprite strategy are documented in `PIXEL_ICON_SYSTEM.md`.

## Current rules

- Existing UI code renders general-purpose icons through `LevelUpIcon` until Sprint 6.1 completes the definitive migration to `PixelIcon`.
- Activity attributes use `ActivityAttributeIcon` or `ActivityAttributeBadge`; Sprint 6.3 will preserve these semantic components while routing rendering through `PixelIcon`.
- Feature components must not introduce new direct SVG file paths.
- Emojis and mixed icon libraries are not allowed in new interface work.
- Only SVG files used by LevelUp are stored under `wwwroot/icons/pixel`.
- Icon colors and badge colors use Design System variables.
- `LevelUpIcon` and `LevelUpIconName` are legacy migration names, not parallel long-term APIs.

## Activity attribute mapping

| Attribute | Icon |
| --- | --- |
| Strength | Bolt |
| Dexterity | Sparkles |
| Intelligence | Book bookmark |
| Wisdom | Book |
| Vitality | Heart |
| Charisma | Crown |

The library is integrated behind reusable components so icon assets can be replaced without changing feature pages.

## Activity attribute polish

Sprint 5.3 completed the attribute presentation layer:

- dashboard filtering by attribute;
- attribute-aware search;
- optional A-Z and Z-A attribute sorting;
- native tooltips and accessible labels on attribute badges;
- responsive filter controls;
- bUnit coverage for the filter bar and attribute accessibility.

Manual order remains the default so drag-and-drop behavior is preserved. Attribute sorting is visual only and does not overwrite the persisted manual order.
