# Shared Components

Design System components are organized by responsibility under `Components/DesignSystem`.

## Groups

- **Attributes**: semantic rendering for activity attributes;
- **Buttons**: standardized actions and icon integration;
- **Cards**: shared card surfaces and card behavior;
- **Feedback**: empty, loading, toast, warning, validation, and confirmation-dialog states;
- **Forms**: labels, fields, selects, validation, checkboxes, and related controls;
- **Icons**: centralized pixel-icon renderer, contracts, registry, and catalog;
- **Layout**: reusable structural surfaces;
- **Modals**: shared dialogs and the editor modal shell;
- **Pages**: development-only visual review surfaces;
- **Text**: typography primitives.

Every editor modal (Habits, Tasks, To-Dos, Projects, Inventory transactions) composes `Modals.EditorModalShell` directly. Every empty state composes `Feedback.LevelUpEmptyState` directly (optionally wrapped by feature-specific composition, e.g. `Inventory.InventoryEmptyState`, which adds contextual actions). Every delete/destructive confirmation composes `Feedback.LevelUpConfirmDialog` directly. There is exactly one component for each of these three contracts — do not reintroduce a feature- or domain-named wrapper that only forwards parameters unchanged.

## Usage rules

- Prefer composition over duplicating markup and CSS.
- Keep business decisions out of Design System components.
- Expose semantic parameters rather than physical asset paths.
- Preserve accessible names for interactive controls.
- Keep decorative icons hidden from assistive technologies.
- Avoid breaking public component parameters without updating all consumers and tests.
- All buttons render through `LevelUpButton` (`Variant`: Primary, Secondary, Success, Warning, Back, Danger, ConfirmationDanger, ConfirmationCancel) — never a bare `<button>` with its own bespoke CSS. Buttons with the same semantic meaning (Save, Cancel, Delete, Edit, Create) use the same variant everywhere. Real UI exceptions (dropdown menu items, combobox triggers, checkboxes, segmented toggles, nav/drawer entries, icon-only dismiss controls, clickable cards) are not "buttons" in this sense and are unaffected.
- Any settings-style page (a card containing a titled form section — Account today; Preferences, Character, and administrative pages in the future) composes `LevelUpSettingsSection` (card + section header) with `LevelUpSettingsForm<TModel>` (EditForm + validation + fieldset + submit action) from the Layout group, rather than re-implementing that card/form chrome per page. Field layout inside the form (grid, hints) uses the shared `.levelup-settings-form__grid` / `.levelup-settings-form__hint` classes from `wwwroot/css/settings.css`.
