# Streamline Pixel — Attribution

BeeDay's official icon library is derived from the Streamline Pixel icon collection.

- **Source**: https://icon-sets.iconify.design/streamline-pixel/
- **Author**: Streamline (https://streamlinehq.com)
- **License**: CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/)

## What this covers

The 662 original SVG files under this folder (`design/icons/source/streamline-pixel/`) are the immutable, unmodified source assets as downloaded from Iconify. A curated subset of them was semantically selected and copied into the application's production icon library at `src/BeeDay.Web/wwwroot/icons/streamline/`, where each file was re-exported as a standalone SVG (identical path/shape data, wrapped for use as an SVG sprite symbol) under BeeDay's own semantic naming — see `docs/design-system/03-icons.md` for the full pipeline and the current source→semantic mapping.

## Compliance

CC BY 4.0 requires attribution wherever the licensed work (or a derivative of it) is used. This file, together with the reference in `docs/design-system/03-icons.md`, is BeeDay's attribution notice for the entire Streamline Pixel–derived icon library used throughout the application.

## Rules for this folder

- These are reference source files only. Never modify, rename, recolor, or delete them.
- Never reference these files directly from application code — the application must only consume icons through `PixelIcon/PixelIconRegistry`, which point at `src/BeeDay.Web/wwwroot/icons/streamline/`.
