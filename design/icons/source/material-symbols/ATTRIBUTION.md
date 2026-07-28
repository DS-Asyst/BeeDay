# Material Symbols — Attribution

LevelUp's primary icon provider for interface, actions, navigation, forms, feedback and general application icons is Google's Material Symbols.

- **Source**: https://fonts.google.com/icons
- **Documentation**: https://developers.google.com/fonts/docs/material_symbols
- **Repository**: https://github.com/google/material-design-icons
- **License**: Apache License 2.0 (https://www.apache.org/licenses/LICENSE-2.0)

## Configuration used

LevelUp uses one consistent Material Symbols configuration for every icon in this folder:

- **Family**: Material Symbols Rounded
- **Fill**: 1 (filled)
- **Weight**: 400
- **Grade**: 0
- **Optical size**: 24

Every file was downloaded from the official repository at the path
`symbols/web/{icon_name}/materialsymbolsrounded/{icon_name}_fill1_24px.svg` and is otherwise unmodified.

## What this covers

The files under this folder (`design/icons/source/material-symbols/`) are the immutable, unmodified source assets as downloaded from Google's repository. A subset was copied into the application's production icon library at `src/LevelUp.Web/wwwroot/icons/material-symbols/`, re-exported as standalone SVG sprite symbols under LevelUp's own semantic naming — see `docs/design-system/icons.md` for the full pipeline and the current source→semantic mapping.

## Compliance

Attribution is not required by the Apache License 2.0, but this file preserves the license notice and source reference as applicable. The upstream `LICENSE` (Apache-2.0) governs use of the icon files themselves.

## Rules for this folder

- These are reference source files only. Never modify, rename, recolor, or delete them.
- Never reference these files directly from application code — the application must only consume icons through `PixelIcon`/`PixelIconRegistry`, which point at `src/LevelUp.Web/wwwroot/icons/material-symbols/`.
