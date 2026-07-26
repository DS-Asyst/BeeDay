# Pixel Icon Library

The Pixel Icon Library by HackerNoon is the official icon source for the LevelUp interface.

## Rules

- UI code must render general-purpose icons through `LevelUpIcon`.
- Activity attributes must use `ActivityAttributeIcon` or `ActivityAttributeBadge`.
- Feature components must not reference SVG file paths directly.
- Emojis and mixed icon libraries are not allowed in new interface work.
- Only the SVG files used by LevelUp are stored under `wwwroot/icons/pixel`.
- Icon colors and badge colors must use Design System variables.

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
