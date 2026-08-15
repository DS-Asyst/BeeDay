# Lucide

The SVG source files in this directory are vendored from the official
[Lucide repository](https://github.com/lucide-icons/lucide). Lucide is licensed under the ISC
License; the unmodified license text is stored alongside the sources in `LICENSE`.

BeeDay uses a curated semantic subset only. `scripts/New-IconSprite.ps1` packages these trusted
outline SVGs into the single production sprite. Feature code must consume them through
`BeeDayIcon`/`BeeDayIconRegistry`, never by referencing provider files directly.
