# LevelUp Web Design System

This folder is the operational source of truth for presentation-layer changes.

Read in order:

1. `foundations.md`
2. `typography.md`
3. `buttons.md`
4. `forms.md`
5. `page-inventory.md`
6. `migration-guide.md`
7. `validation.md`

## Governing principles

- Extend existing Design System components before creating new controls.
- Use semantic tokens instead of physical font names or literal colors.
- Do not use `!important` to solve ordinary component styling.
- Keep scoped page CSS responsible for layout, not global component policy.
- Preserve keyboard, focus, disabled and loading behavior.
- Migrate incrementally; remove legacy rules only after proving there are no consumers.
