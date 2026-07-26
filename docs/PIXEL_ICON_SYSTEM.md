# Pixel Icon System

## Status

This document defines the architectural foundation for Epic 6 — Pixel Icon System.
Sprint 6.0 is a planning and repository-normalization sprint. It does not perform the broad visual migration reserved for Sprints 6.1 through 6.6.

## Existing infrastructure

LevelUp already contains an initial pixel-icon implementation:

- `LevelUpIcon.razor` and `LevelUpIcon.razor.cs` render general-purpose icons;
- `LevelUpIconName` defines the current public icon names;
- SVG assets are stored in `src/LevelUp.Web/wwwroot/icons/pixel`;
- `LevelUpButton` can render `LevelUpIcon` instances;
- `FilterBar` already uses the general-purpose component;
- `ActivityAttributeIcon` and `ActivityAttributeBadge` provide attribute-specific presentation;
- bUnit coverage exists for `LevelUpIcon`, `LevelUpButton`, and activity-attribute icons;
- `docs/PIXEL_ICON_LIBRARY.md` records the initial library rules.

This infrastructure is the migration baseline. Epic 6 must evolve it rather than create a competing icon subsystem.

## Architectural decision

The final public API is:

```text
Feature / Layout Component
           |
           v
       PixelIcon
           |
           v
   Pixel Icon Registry
           |
           v
    Pixel SVG Library
```

The migration is definitive:

```text
LevelUpIcon       -> PixelIcon
LevelUpIconName   -> PixelIconName
```

`LevelUpIcon` and `LevelUpIconName` may exist only as temporary migration artifacts inside Sprint 6.1. They must be removed before that sprint is accepted. The project must not retain two general-purpose icon systems.

## Current implementation constraints

The initial component currently:

- accepts an arbitrary integer size;
- builds most asset names from the enum value;
- renders an external SVG through an `<image>` element;
- does not expose semantic color tokens;
- does not use a central icon registry or sprite;
- relies on the caller to provide a label for informative icons.

Sprint 6.1 will replace those constraints with the official contracts: `PixelIconName`, `PixelIconSize`, `PixelIconColor`, a central registry, sprite-based loading, fallback behavior, and accessibility validation.

## Initial inventory

### General-purpose SVG assets

| Current name | Physical asset | Category | Known consumers | Status | Duplication risk | Migration action | Planned `PixelIconName` |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Add | `icons/pixel/add.svg` | Actions | `FilterBar`, `LevelUpButton` | Used | Low | Register and migrate | `Add` |
| Edit | `icons/pixel/edit.svg` | Actions | Available through `LevelUpIconName` | Available | Low | Register | `Edit` |
| Delete | `icons/pixel/delete.svg` | Actions | Available through `LevelUpIconName` | Available | Medium: delete dialogs and inline symbols | Consolidate | `Delete` |
| Save | `icons/pixel/save.svg` | Actions | `LevelUpButton` tests and API | Used | Low | Register and migrate | `Save` |
| Close | `icons/pixel/close.svg` | Actions | Available through `LevelUpIconName` | Available | High: textual multiplication signs and inline SVGs remain | Consolidate | `Close` |
| Search | `icons/pixel/search.svg` | Actions | `FilterBar` | Used | Low | Register and migrate | `Search` |
| Check | `icons/pixel/check.svg` | Feedback | Available through `LevelUpIconName` | Available | High: checkmark text symbols remain | Consolidate | `Check` |
| More | `icons/pixel/more.svg` | Actions | Available through `LevelUpIconName` | Available | Low | Register | `More` |
| Warning | `icons/pixel/warning.svg` | Feedback | `LevelUpIcon` tests | Tested | Low | Register and migrate | `Warning` |
| Info | `icons/pixel/info.svg` | Feedback | Available through `LevelUpIconName` | Available | Low | Register | `Information` |
| Settings | `icons/pixel/settings.svg` | System | Available through `LevelUpIconName` | Available | Medium: inline navigation SVGs remain | Consolidate | `Settings` |
| User | `icons/pixel/user.svg` | Navigation | Available through `LevelUpIconName` | Available | Medium: account panel inline SVGs remain | Consolidate | `Account` |
| Lock | `icons/pixel/lock.svg` | System | Available through `LevelUpIconName` | Available | Medium: account panel inline SVGs remain | Consolidate | `Lock` |
| Language | `icons/pixel/language.svg` | Navigation | Available through `LevelUpIconName` | Available | Medium: account panel inline SVGs remain | Consolidate | `Language` |
| Chevron Down | `icons/pixel/chevron-down.svg` | Navigation | Available through `LevelUpIconName` | Available | Low | Register | `ChevronDown` |
| Chevron Left | `icons/pixel/chevron-left.svg` | Navigation | Available through `LevelUpIconName` | Available | Low | Register | `ChevronLeft` |
| Chevron Right | `icons/pixel/chevron-right.svg` | Navigation | Available through `LevelUpIconName` | Available | Low | Register | `ChevronRight` |
| Inventory | `icons/pixel/inventory.svg` | Navigation | `LevelUpIcon` tests and enum | Tested | Medium: navigation inline SVGs remain | Consolidate | `Inventory` |
| Book | `icons/pixel/book.svg` | Navigation | Available through `LevelUpIconName` | Available | Medium: library navigation icon may be inline | Consolidate | `Library` |
| Daily | `icons/pixel/daily.svg` | Navigation | Available through `LevelUpIconName` | Available | Medium: top navigation inline SVG remains | Consolidate | `Daily` |

### Activity attribute SVG assets

| Current name | Physical asset | Category | Known consumers | Status | Duplication risk | Migration action | Planned `PixelIconName` |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Strength | `icons/pixel/attribute-strength.svg` | Attributes | `ActivityAttributeIcon`, badges, tests | Used | Low | Route semantic component through `PixelIcon` | `AttributeStrength` |
| Dexterity | `icons/pixel/attribute-dexterity.svg` | Attributes | `ActivityAttributeIcon`, badges, tests | Used | Low | Route semantic component through `PixelIcon` | `AttributeDexterity` |
| Intelligence | `icons/pixel/attribute-intelligence.svg` | Attributes | `ActivityAttributeIcon`, badges, tests | Used | Low | Route semantic component through `PixelIcon` | `AttributeIntelligence` |
| Wisdom | `icons/pixel/attribute-wisdom.svg` | Attributes | `ActivityAttributeIcon`, badges, tests | Used | Low | Route semantic component through `PixelIcon` | `AttributeWisdom` |
| Vitality | `icons/pixel/attribute-vitality.svg` | Attributes | `ActivityAttributeIcon`, badges, tests | Used | Low | Route semantic component through `PixelIcon` | `AttributeVitality` |
| Charisma | `icons/pixel/attribute-charisma.svg` | Attributes | `ActivityAttributeIcon`, badges, tests | Used | Low | Route semantic component through `PixelIcon` | `AttributeCharisma` |

### Known direct or parallel icon usage

The initial audit found the following migration targets:

| Location | Current pattern | Planned sprint |
| --- | --- | --- |
| `Components/Layout/AccountSidePanel.razor` | Multiple inline SVGs, including social logos | 6.2 |
| `Components/Layout/TopNavigation.razor` | Inline SVG | 6.2 |
| `Components/Layout/AppFooter.razor` | Inline social SVGs | 6.2 |
| `Components/DesignSystem/Attributes/ActivityAttributeIcon.razor` | Separate SVG renderer and direct asset path | 6.3 |
| `Components/DesignSystem/Feedback/LevelUpToastHost.razor` | Textual close and success symbols | 6.4 |
| `Components/Features/Onboarding/Pages/Tutorial.razor` | Textual check symbols | 6.3 or 6.6 |
| `Components/Features/Dashboard/Components/ActivityPreviewCard.razor` | Textual check symbol | 6.3 |
| `Components/Features/Projects/Components/ProjectWorkspace.razor` | Textual close symbol | 6.3 |
| `Components/Features/Todos/Components/TodoEditorModal.razor` | Textual check symbol | 6.4 |

This audit is intentionally descriptive. Sprint 6.0 does not replace these icons.

## Categories

The initial registry taxonomy is:

```text
Actions
Activities
Attributes
Feedback
Navigation
Social
Statistics
System
```

A category describes design-system ownership and catalog organization. It must not leak physical paths into feature components.

## Naming rules

- Public names describe semantics, not filenames or visual shapes.
- Compound names use PascalCase in C# and kebab-case for physical symbols when needed.
- Domain ambiguity must be avoided, for example `RecurringTask` and `Todo` rather than an overloaded `Task` name.
- Social brand names use their official recognizable names.
- Attribute icons use the `Attribute` prefix.
- Renaming a physical file must not require changes in feature components; the registry owns that mapping.

## Asset organization target

Sprint 6.1 will organize the library under:

```text
wwwroot/icons/pixel/
|-- actions/
|-- activities/
|-- attributes/
|-- feedback/
|-- navigation/
|-- social/
|-- statistics/
`-- system/
```

The migration must be performed together with registry updates so no feature component depends on these paths.

## Cache and sprite strategy

The target renderer uses an SVG sprite reference:

```html
<svg>
    <use href="/icons/pixel/sprite.svg#search"></use>
</svg>
```

The browser's HTTP cache is the primary cache. The application must not read and parse an SVG file on every component render. A build-time or controlled static sprite is preferred over per-request dynamic loading.

The registry maps `PixelIconName` to the sprite symbol and metadata. Missing registrations or symbols must render a stable fallback without breaking the page.

## Accessibility policy

Decorative icons must render with:

```html
aria-hidden="true"
focusable="false"
```

Informative icons must render with an accessible name and image semantics:

```html
role="img"
aria-label="..."
```

An informative icon without a non-empty label is invalid. Interactive controls must receive their accessible name from the control itself; an icon inside a labeled button remains decorative.

Color alone must not be the only carrier of state. Focus behavior, contrast, and reduced-motion preferences are reviewed during Sprint 6.6.

## Sprint 6.4 boundary

Sprint 6.4 is limited to icon integration with the existing Forms and Dialogs components.

It includes icons for close, save, cancel, delete, confirmation, validation, calendar, select, checkbox state, warning, information, success, and loading.

It explicitly excludes:

- a general forms refactor;
- validation-rule changes;
- a complete input redesign;
- modal architecture changes;
- editing-flow restructuring;
- a new forms framework;
- business-rule, persistence, or navigation changes.

## Epic acceptance invariants

At the end of Epic 6:

- `PixelIcon` is the only general-purpose icon renderer;
- `PixelIconName` is the only general-purpose icon-name contract;
- feature and layout components do not reference physical SVG paths;
- feature and layout components do not contain inline SVG markup;
- semantic wrappers such as `ActivityAttributeIcon` delegate rendering to `PixelIcon`;
- sizes and colors use Design System tokens;
- informative and decorative icons follow the accessibility policy;
- duplicate and unused assets are removed;
- bUnit tests cover rendering, tokens, accessibility, sprite references, and fallback behavior;
- the repository line-ending policy explicitly includes SVG files.

## Sprint 6.0 acceptance record

Sprint 6.0 establishes the migration decision, inventory, scope boundary, target architecture, and repository rule. It intentionally leaves the current runtime implementation unchanged for Sprint 6.1.
