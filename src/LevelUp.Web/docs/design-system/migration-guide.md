# Migration Guide

## Completed foundation changes

- Reordered CSS so typography policy no longer overrides scoped component styles.
- Removed global typography `!important` declarations.
- Changed the legacy default family alias to Inter for readable content.
- Added semantic typography roles and utilities.
- Added `LevelUpButtonTypography` without breaking existing button consumers.
- Migrated the Login `Sign in` action to explicit pixel typography.
- Removed duplicate global authentication styling from `app.css`; authentication pages own their scoped visuals.

## Migrating a page

1. Record the page at desktop and mobile widths.
2. Identify shared controls already available.
3. Replace direct font-family declarations with semantic roles.
4. Replace literal palette values with existing tokens; add a token only when the meaning is reusable.
5. Keep page CSS for layout and composition.
6. Test hover, focus-visible, active, disabled, loading, validation and reduced motion.
7. Run the validation commands in `validation.md`.

## Compatibility

Do not delete `--levelup-font-family` yet. It remains a migration alias. Do not rename existing button variants or CSS modifier classes without explicit approval.
