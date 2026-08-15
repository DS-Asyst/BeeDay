# Devicon — Attribution

BeeDay uses Devicon only for supported brands, technologies and development tools that Devicon actually provides.

- **Source**: https://devicon.dev/
- **Repository**: https://github.com/devicons/devicon
- **License**: MIT (see `LICENSE` in this folder — the MIT copyright and permission notice must be preserved)

## What this covers

The files under this folder (`design/icons/source/devicon/`) are the immutable, unmodified source assets as downloaded from the Devicon repository (`icons/{name}/{name}-original.svg`). A subset was copied into the application's production icon library at `src/BeeDay.Web/wwwroot/icons/devicon/`, re-exported as standalone SVG sprite symbols under BeeDay's own semantic naming — see `docs/design-system/03-icons.md` for the full pipeline and the current source→semantic mapping.

Currently used: GitHub, Facebook, LinkedIn.

## Trademark notice

Devicon's MIT license covers the SVG markup/artwork files themselves. The brand marks depicted (GitHub, Facebook, LinkedIn, and any other brand or technology logo) remain the trademarks of their respective owners and are subject to each owner's own brand and trademark policy, independent of Devicon's MIT license. Do not imply endorsement by, or an affiliation with, any brand owner beyond accurately representing a link to that brand's official presence.

## Rules for this folder

- These are reference source files only. Never modify, rename, or delete them. Recoloring is only acceptable where a brand's own guidelines explicitly allow monochrome/single-color use.
- Never reference these files directly from application code — the application must only consume icons through `BeeDayIcon`/`BeeDayIconRegistry`, which point at `src/BeeDay.Web/wwwroot/icons/devicon/`.
