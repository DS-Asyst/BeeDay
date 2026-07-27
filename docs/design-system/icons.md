# Pixel Icon System

LevelUp uses one general-purpose icon renderer: `PixelIcon`.

## Core contracts

- `PixelIconName`: semantic icon identifier;
- `PixelIconSize`: supported size contract;
- `PixelIconColor`: semantic color contract;
- `PixelIconRegistry`: maps semantic names to sprite symbols and metadata;
- `PixelIcon`: renders the official SVG sprite reference.

Physical asset paths must not leak into feature components.

## Assets

Source assets and the generated sprite live under:

```text
src/LevelUp.Web/wwwroot/icons/pixel
```

The authenticated Development catalog is available at:

```text
/design-system/icons
```

## Accessibility

- Decorative icons use `aria-hidden="true"` and are not focusable.
- Informative icons require a non-empty accessible label.
- Icons inside controls are usually decorative when the control already has visible text or an accessible name.
- An unknown enum value falls back to the Warning icon rather than producing broken markup.

## Repository policy

- Do not embed general-purpose `<svg>` markup in feature Razor components.
- Do not reference `.svg` paths directly from feature code.
- Add semantic names and registry entries when extending the system.
- Keep source assets, sprite symbols, registry entries, and tests synchronized.
