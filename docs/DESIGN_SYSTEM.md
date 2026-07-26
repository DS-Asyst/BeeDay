# LevelUp Design System

The LevelUp Design System provides the reusable UI contracts used by the Blazor application. Components must consume semantic tokens and shared renderers instead of embedding visual assets or feature-specific variants.

## Pixel Icon System

`PixelIcon` is the only official icon renderer. Feature components must reference `PixelIconName` contracts and must not use inline SVG, direct `.svg` paths, icon fonts, or functional emojis.

```razor
<PixelIcon Name="PixelIconName.Inventory" />
```

### Size

Use the `PixelIconSize` enum:

- `ExtraSmall`: 12 px
- `Small`: 16 px
- `Medium`: 20 px
- `Large`: 24 px
- `ExtraLarge`: 32 px

```razor
<PixelIcon Name="PixelIconName.Level" Size="PixelIconSize.Large" />
```

### Color

Use `PixelIconColor` semantic tokens. Colors communicate interface meaning or inherit the surrounding text color. Do not assign arbitrary hexadecimal values to icons.

```razor
<PixelIcon Name="PixelIconName.Income" Color="PixelIconColor.Success" />
<PixelIcon Name="PixelIconName.Expense" Color="PixelIconColor.Danger" />
```

### Accessibility

Decorative icons are hidden from assistive technology by default:

```razor
<PixelIcon Name="PixelIconName.Add" />
```

Informative icons require an explicit accessible label:

```razor
<PixelIcon Name="PixelIconName.Warning"
           Decorative="false"
           Label="Warning status"
           Color="PixelIconColor.Warning" />
```

Do not repeat information already present in adjacent visible text. In that case, keep the icon decorative.

### Adding an icon

1. Add the semantic enum member to `PixelIconName`.
2. Place the optimized 24 × 24 SVG under `wwwroot/icons/pixel/<category>/`.
3. Add one definition to `PixelIconRegistry`.
4. Add a symbol with the same ID to `sprite.svg`.
5. Add registry, rendering, and accessibility coverage to `PixelIconTests`.
6. Review the icon in `/design-system/icons` while running in Development.
7. Run formatting, build, tests, and the icon-system audit commands.

### Visual catalog

The internal catalog is available at:

```text
/design-system/icons
```

It lists every contract, category, label, asset, size, color, decorative example, informative example, and usage snippet. The route is restricted to authenticated users and only exposes the catalog content in the Development environment.

## Interaction consistency

Icons inherit the interaction state of their host component. Disabled controls reduce icon opacity. Motion is subtle and is disabled when the operating system requests reduced motion through `prefers-reduced-motion`.

## Prohibited patterns

- `<svg>` inside feature Razor components.
- `<img src="...svg">` in pages or components.
- CSS `url(...svg)` references outside the official sprite infrastructure.
- `LevelUpIcon` or `LevelUpIconName`.
- Functional emojis used as buttons, statuses, navigation, or indicators.
- New icon renderers that bypass `PixelIcon`.
