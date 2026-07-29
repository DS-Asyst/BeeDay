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

## Page Header vs. Hero (`LevelUpPageHeader` vs. `LevelUpHero`)

Both live in the Layout group and share the same responsive flex-header CSS foundation (`design-system.css`/`polish.css`), so choosing between them is about responsibility, not styling:

- **`LevelUpPageHeader`** (and `LevelUpSectionHeader` for a sub-section within a page): a plain introductory header for an operational, authenticated page — title, optional eyebrow, optional description, and a generic actions row. No illustration, no variant, no distinguished single primary action. This is `Account.razor`'s current header today, and remains the right choice for settings/utility pages. Its public contract is unchanged by this sprint.
- **`LevelUpHero`**: a richer introductory panel for a page that needs one or more of: an illustration, a single emphasized primary action, brief supporting context, or a contextual visual variant (`Default` or `Onboarding`). Reach for `LevelUpPageHeader` first; only use `LevelUpHero` when the page genuinely needs one of those additional capabilities.

`LevelUpHero`'s public contract:

```text
Title (string, required)
Eyebrow (string?)
Subtitle (string?)
Illustration (RenderFragment?)
PrimaryAction (RenderFragment?)
SupportingContent (RenderFragment?)
Variant (LevelUpHeroVariant: Default | Onboarding)
Class (string?)
AdditionalAttributes
```

Every optional parameter renders no wrapper element at all when omitted — there are no empty `<div>`s to style around.

**Asset strategy**: `LevelUpHero` never selects, loads, resolves, or paths an asset itself, and never infers alternative text. `Illustration` is an opaque `RenderFragment` — the consumer composes an existing `PixelIcon`, an SVG, an image, or another visual component inside it, and that consumer owns the accessibility of whatever they put there:

- **Decorative illustration**: compose a `PixelIcon` with `Decorative="true"` (the default) — hidden from assistive technology, no label.
- **Informative illustration**: compose a `PixelIcon` with `Decorative="false"` and an explicit `Label` — exposed with `role="img"` and an accessible name.

See `/design-system/hero` (Development only) for both examples rendered side by side, plus a Default-variant and an Onboarding-variant composition.

**Supporting content restrictions**: `SupportingContent` is for short, auxiliary contextual information only (a one- or two-line note, a small checklist item) — never a form, grid, table, long list, complex layout, or feature/business data section. If a page needs any of those, they belong in the page body below the hero, not inside it.

**What `LevelUpHero` is not**: it is not a page builder. It has no `ChildContent`, performs no data loading, no navigation, and contains no business logic — it is presentation-only, entirely inside the Web Design System, with no Domain/Application/Infrastructure dependency. It reuses existing tokens, `LevelUpButton`, and `PixelIcon` rather than introducing new visual primitives.

**Responsive behavior**: the header row (illustration, title block, primary action) stacks vertically under 42rem, matching `LevelUpPageHeader`'s existing breakpoint; the illustration is hidden entirely below that width so the title and primary action stay immediately visible and usable on mobile without added scrolling. Supporting content always renders full-width below the row.

**Current adoption**: no production page consumes `LevelUpHero` yet. It ships with isolated, realistic usage examples at `/design-system/hero` (Development only, `[Authorize]`-gated, not linked from any navigation) — following the same precedent as the Pixel Icon Catalog. No existing page header was replaced to create this consumer.
