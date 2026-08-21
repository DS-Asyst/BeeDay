# Design System Change Log

## 2026-07-29 — Typography and component foundation

- Established semantic typography roles.
- Corrected stylesheet ordering.
- Removed global `!important` font enforcement.
- Added explicit button typography selection.
- Migrated Login Sign in button.
- Removed duplicated authentication styles from `app.css`.
- Added design-system operating documentation.

This package intentionally preserves existing variants and feature behavior. Page-by-page visual migration should follow the current [`docs/design-system/README.md`](../../docs/design-system/README.md) — the `migration-guide.md` this entry originally pointed to was never actually committed (confirmed via `git log --follow`, Sprint 30.26).
