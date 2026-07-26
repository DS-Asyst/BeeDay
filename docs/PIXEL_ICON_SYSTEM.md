# Pixel Icon System

## Status

Sprint 6.1 established the official Pixel Icon System. The project now has one general-purpose renderer, one icon-name contract, an explicit registry, semantic size and color tokens, categorized SVG sources, and a single browser-cacheable sprite.

The initial icon implementation was used only as the migration baseline and has been removed. There is no legacy compatibility renderer and no parallel general-purpose icon enum.

## Architecture

```text
Feature / Layout Component
           |
           v
       PixelIcon
           |
           v
   PixelIconRegistry
           |
           v
 /icons/pixel/sprite.svg
           |
           v
 Categorized SVG Sources
```

Feature and layout components select a semantic `PixelIconName`. They do not calculate paths, know filenames, or read files.

## Contracts

```text
Components/DesignSystem/Icons/
├── PixelIcon.razor
├── PixelIcon.razor.cs
├── PixelIcon.razor.css
├── PixelIconName.cs
├── PixelIconSize.cs
├── PixelIconColor.cs
├── PixelIconCategory.cs
├── PixelIconDefinition.cs
└── PixelIconRegistry.cs
```

### PixelIconName

Defines the public semantic vocabulary. Physical file and symbol names are intentionally independent from enum names.

### PixelIconDefinition

Each definition records:

- sprite symbol identifier;
- categorized source asset path;
- category;
- semantic name;
- optional default label metadata;
- fallback metadata.

### PixelIconRegistry

The registry is the only mapping from `PixelIconName` to physical metadata. Unknown enum values resolve to the official warning fallback and do not break rendering.

### PixelIconSize

Sizes are restricted to five Design System tokens: 12, 16, 20, 24, and 32 pixels.

### PixelIconColor

Colors are semantic tokens resolved through Design System variables. Attribute colors use the same semantic palette already applied by attribute badges.

## Sprite and cache strategy

The component renders an SVG `<use>` reference:

```html
<svg>
  <use href="/icons/pixel/sprite.svg#search"></use>
</svg>
```

The sprite contains one symbol for every registered source asset. This provides:

- one cacheable HTTP resource;
- no server-side file read during rendering;
- no repeated SVG body in component markup;
- centralized symbol replacement;
- color inheritance through `currentColor`.

The categorized source SVGs remain the maintainable originals. The sprite is the runtime delivery artifact.

## Accessibility policy

### Decorative icons

Decorative is the default:

```html
aria-hidden="true"
focusable="false"
```

### Informative icons

Informative icons render:

```html
role="img"
aria-label="..."
focusable="false"
```

A non-empty `Label` is mandatory when `Decorative` is `false`. Parameter validation throws before an unlabeled informative icon can render.

## Fallback policy

`PixelIconRegistry.Resolve` returns the `Warning` definition for an unknown enum value. Registered assets and sprite symbols are generated and reviewed together, preventing normal runtime references to missing source files.

The fallback is intentionally visual and non-fatal. Decorative behavior remains unchanged; an informative fallback still requires an explicit accessible label.

## Asset inventory

| Semantic name | Source asset | Category | Sprite symbol |
| --- | --- | --- | --- |
| Add | `actions/add.svg` | Actions | `add` |
| Edit | `actions/edit.svg` | Actions | `edit` |
| Delete | `actions/delete.svg` | Actions | `delete` |
| Save | `actions/save.svg` | Actions | `save` |
| Close | `actions/close.svg` | Actions | `close` |
| Search | `actions/search.svg` | Actions | `search` |
| More | `actions/more.svg` | Actions | `more` |
| Check | `feedback/check.svg` | Feedback | `check` |
| Warning | `feedback/warning.svg` | Feedback | `warning` |
| Information | `feedback/info.svg` | Feedback | `info` |
| Settings | `system/settings.svg` | System | `settings` |
| Lock | `system/lock.svg` | System | `lock` |
| Account | `navigation/user.svg` | Navigation | `user` |
| Language | `navigation/language.svg` | Navigation | `language` |
| Chevron Down | `navigation/chevron-down.svg` | Navigation | `chevron-down` |
| Chevron Left | `navigation/chevron-left.svg` | Navigation | `chevron-left` |
| Chevron Right | `navigation/chevron-right.svg` | Navigation | `chevron-right` |
| Inventory | `navigation/inventory.svg` | Navigation | `inventory` |
| Library | `navigation/book.svg` | Navigation | `book` |
| Daily | `navigation/daily.svg` | Navigation | `daily` |
| Strength | `attributes/attribute-strength.svg` | Attributes | `attribute-strength` |
| Dexterity | `attributes/attribute-dexterity.svg` | Attributes | `attribute-dexterity` |
| Intelligence | `attributes/attribute-intelligence.svg` | Attributes | `attribute-intelligence` |
| Wisdom | `attributes/attribute-wisdom.svg` | Attributes | `attribute-wisdom` |
| Vitality | `attributes/attribute-vitality.svg` | Attributes | `attribute-vitality` |
| Charisma | `attributes/attribute-charisma.svg` | Attributes | `attribute-charisma` |

## Existing integrations migrated in Sprint 6.1

- `LevelUpButton`
- dashboard `FilterBar`
- `ActivityAttributeIcon`
- `ActivityAttributeBadge`
- icon bUnit tests
- button and attribute-component bUnit tests

`ActivityAttributeIcon` remains as a semantic domain adapter, but delegates all general-purpose rendering to `PixelIcon`.

## Sprint 6.4 boundary

Sprint 6.4 is restricted to icon integration with existing forms and dialogs. It must not redesign form APIs, change validation rules, replace modal architecture, restructure editing flows, or create a new forms framework.

## Repository rules

SVG source files are text assets and follow:

```gitattributes
*.svg text eol=crlf
```

The generated sprite follows the same rule. Repository normalization must be reviewed before commit.

## Acceptance checks

- only `PixelIcon` is a general-purpose renderer;
- no legacy icon contracts remain in source or tests;
- every enum value has an explicit registry entry;
- all official sizes are covered by tests;
- semantic color classes are covered by tests;
- decorative and informative accessibility are covered by tests;
- missing labels are rejected;
- unknown icon names use the warning fallback;
- sprite references are covered by tests;
- source assets and sprite symbols remain synchronized.


## Sprint 6.2 — Navigation migration

Navigation and layout surfaces now consume the official `PixelIcon` renderer. The Top Navigation, account/support drawer, and application footer no longer embed SVG markup or reference physical SVG paths. Social icons are registered in the `Social` category, while menu, character, support, donation, and logout icons are registered in `Navigation`.

Layout components may style the rendered icon through wrapper elements and `::deep` selectors, but they must not know the sprite path or asset path. Interactive controls retain their text or `aria-label`; embedded icons remain decorative unless they are the sole accessible content.


## Sprint 6.3 — Activity Icons

Activity types and common activity actions now use `PixelIcon`, including cards, context menus, search/create controls, editor actions, project workspace controls, completion, repetition, tags, attributes, calendar and filters.


## Sprint 6.4 — Dialog & Forms Icon Integration

Completed: modal close/save/cancel/delete actions, confirmation and warning states, validation feedback, select/date/checkbox controls, toast states and loading now use `PixelIcon` without changing form contracts or business rules.


## Sprint 6.5 — Dashboard & Statistics Icons

- Existing character, XP, level, wallet, inventory and transaction indicators use `PixelIcon`.
- Added semantic statistics contracts for experience, level, wallet, income, expense, trends, streak, completed and pending states.
- No dashboard, statistics page, library implementation, wallet rule or functional module was introduced.
- Status colors communicate semantic state: success for income, danger for expense, and neutral/default for informational metrics.
