# User Interface

`LevelUp.Web` is a Blazor Server application organized by feature under `Components/Features` and by reusable primitives under `Components/DesignSystem`.

## Feature areas

- Account
- Authentication and identity
- Character and character creation
- Dashboard
- Habits
- Recurring tasks
- Todos
- Projects
- Inventory

## Organization

Feature folders may contain:

- `Pages`: routable components;
- `Components`: feature-specific presentation components;
- `Models`: UI models and form models;
- `State`: scoped page or workflow state;
- `Services`: presentation-layer orchestration that does not belong in Application.

Global layout and reusable visual primitives remain outside feature folders.

## Design principles

- pixel-art-inspired visual language;
- reusable controls for buttons, cards, forms, icons, feedback, modals, and text;
- consistent loading, empty, error, disabled, and confirmation states;
- responsive layouts and touch-friendly targets;
- visible keyboard focus and semantic labels;
- reduced-motion support for nonessential animation;
- feature state isolated from persistence implementation details.

## Typography

Official families:

- **Jersey 15**: brand, primary headings, card titles, large numeric indicators, and decorative RPG elements;
- **Jersey 25**: buttons, inputs, labels, body copy, menus, tables, dialogs, messages, navigation, and actions.

Canonical scale:

| Token | Size | Intended use |
| --- | ---: | --- |
| `--levelup-font-size-xs` | `.75rem` | metadata and compact supporting text |
| `--levelup-font-size-sm` | `.85rem` | labels and secondary text |
| `--levelup-font-size-md` | `.95rem` | regular interface text |
| `--levelup-font-size-base` | `1rem` | body text |
| `--levelup-font-size-lg` | `1.125rem` | emphasized text |
| `--levelup-font-size-xl` | `1.5rem` | card and section headings |
| `--levelup-font-size-2xl` | `1.8rem` | page headings |
| `--levelup-font-size-3xl` | `2.2rem` | display headings |

Textual controls should not be smaller than `.75rem`.

## Character experience panel

Character progression is shown in the existing left-side character panel. It displays:

- current level;
- current-level XP;
- XP required for the next level;
- responsive progress bar;
- temporary gain feedback only when persisted total XP increases.

Daily activity counters are intentionally excluded from this panel. Idempotent reward attempts do not replay XP feedback. Motion is disabled when the user prefers reduced motion.

## Inventory architecture

The Inventory page remains an orchestrator. Supporting responsibilities are split across:

- `State/InventoryPageState.cs` for filter and pagination state;
- `Components/WalletSummary.razor` for wallet totals;
- `Components/TransactionList.razor` and `TransactionCard.razor` for transaction presentation;
- `Components/TransactionFormModal.razor` for transaction input;
- `Components/InventoryFilters.razor` for filtering and sorting controls;
- `Components/InventoryEmptyState.razor` for first-use and no-result states;
- `Components/InventoryTagManager.razor` for tag management.

Inventory uses guarded interaction state to prevent concurrent mutations and duplicate submissions. Forms preserve user input after failed saves, destructive actions require confirmation, and data is refreshed after successful server-side mutation.

Search and filtering support description or notes, transaction type, tag, inclusive date range, sort direction, active-filter counting, reset, and pagination reset after filter changes.

## UI tests

The Web test project uses bUnit for public component behavior, including:

- loading and empty states;
- form validation and disabled states;
- Inventory filtering, pagination, and interaction guards;
- wallet and transaction rendering;
- character experience values, progress, loading, and gain feedback.

Tests should avoid coupling to internal CSS implementation unless a visual class is itself a public contract.
