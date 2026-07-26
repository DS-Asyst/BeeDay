# Pixel Icon Library

The Pixel Icon Library is the official icon source for LevelUp. Runtime components must render general-purpose icons through `PixelIcon`; feature and layout components must not calculate asset paths or render library SVG markup directly.

## Public API

The library is exposed through:

- `PixelIcon`
- `PixelIconName`
- `PixelIconSize`
- `PixelIconColor`
- `PixelIconCategory`
- `PixelIconDefinition`
- `PixelIconRegistry`

The former general-purpose icon contracts were removed during Sprint 6.1. No compatibility component or parallel icon enum remains.

## Rendering

`PixelIcon` renders a reference to the cached sprite:

```razor
<PixelIcon Name="PixelIconName.Search"
           Size="PixelIconSize.Medium"
           Color="PixelIconColor.Primary" />
```

The generated markup references:

```text
/icons/pixel/sprite.svg#search
```

The browser downloads and caches one sprite instead of loading the same SVG source repeatedly.

## Accessibility

Icons are decorative by default and render with `aria-hidden="true"` and `focusable="false"`.

Informative icons must explicitly disable decorative mode and provide a label:

```razor
<PixelIcon Name="PixelIconName.Warning"
           Decorative="false"
           Label="Warning status" />
```

Rendering an informative icon without a non-empty label throws an `InvalidOperationException` during component parameter validation.

## Official sizes

| Token | Pixels |
| --- | ---: |
| `ExtraSmall` | 12 |
| `Small` | 16 |
| `Medium` | 20 |
| `Large` | 24 |
| `ExtraLarge` | 32 |

Arbitrary integer sizes are not part of the component contract.

## Semantic colors

Available tokens are:

- `Current`
- `Primary`
- `Secondary`
- `Muted`
- `Success`
- `Warning`
- `Danger`
- `Information`
- `Strength`
- `Dexterity`
- `Intelligence`
- `Wisdom`
- `Vitality`
- `Charisma`

Tokens resolve to Design System CSS variables. Feature components should prefer `Current` when the surrounding component already controls the icon color.

## Asset organization

```text
wwwroot/icons/pixel/
├── actions/
├── activities/
├── attributes/
├── feedback/
├── navigation/
├── social/
├── statistics/
├── system/
└── sprite.svg
```

Only categories with available assets contain files. Empty categories are reserved for later EPIC 6 sprints.

## Adding an icon

1. Place the source SVG in the correct category.
2. Use a lowercase kebab-case filename and a `0 0 24 24` view box.
3. Add an intentional value to `PixelIconName`.
4. Add an explicit `PixelIconDefinition` to `PixelIconRegistry`.
5. Add the SVG as a symbol in `sprite.svg` using the same symbol identifier.
6. Add or update bUnit tests.
7. Confirm the SVG obeys `.gitattributes` and contains no unsafe external resources.

Components must never infer filenames from enum values.
