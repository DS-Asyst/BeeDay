# User Interface

LevelUp.Web is a Blazor Server application organized by feature under `Components/Features` and by reusable primitives under `Components/DesignSystem`.

## Feature Areas

- Account
- Authentication and identity
- Character creation and onboarding
- Dashboard
- Habits
- Recurring tasks
- Todos
- Projects
- Inventory

## Design Principles

- Pixel-art-inspired visual language
- Reusable components for buttons, cards, forms, icons, feedback, modals, and text
- Consistent loading, empty, error, and disabled states
- Responsive layouts
- Accessible labels and keyboard behavior
- Feature state isolated from persistence details

## Typography

- **Jersey 15**: branding, primary headings, card titles, large numeric indicators, and decorative RPG elements
- **Jersey 25**: buttons, inputs, labels, body copy, menus, tables, dialogs, messages, navigation, and actions

### Canonical Scale

| Token | Size | Intended use |
|---|---:|---|
| `--levelup-font-size-xs` | `.75rem` | Metadata and compact supporting text |
| `--levelup-font-size-sm` | `.85rem` | Labels and secondary text |
| `--levelup-font-size-md` | `.95rem` | Regular interface text |
| `--levelup-font-size-base` | `1rem` | Body text |
| `--levelup-font-size-lg` | `1.125rem` | Emphasized text |
| `--levelup-font-size-xl` | `1.5rem` | Card and section headings |
| `--levelup-font-size-2xl` | `1.8rem` | Page headings |
| `--levelup-font-size-3xl` | `2.2rem` | Display headings |

Textual controls should not be smaller than `.75rem`.

## Integration

The presentation layer accesses application behavior through registered services and MediatR handlers. JSON persistence remains an Infrastructure responsibility.

## Run

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

## Inventory component architecture

The Inventory feature is organized under `Components/Features/Inventory` and keeps the page focused on orchestration.

- `Pages/Inventory.razor`: loads feature data, coordinates actions, controls dialogs, and exposes global feedback.
- `State/InventoryPageState.cs`: owns filter and pagination state.
- `Components/WalletSummary.razor`: renders wallet totals.
- `Components/TransactionList.razor`: renders the transaction panel, list, and pagination.
- `Components/TransactionCard.razor`: renders one transaction and its actions.
- `Components/TransactionFormModal.razor`: handles transaction form presentation.
- `Components/InventoryFilters.razor`: owns filter controls and emits filter changes.
- `Components/InventoryEmptyState.razor`: renders filtered and first-use empty states.
- `Components/InventoryTagManager.razor`: manages tag presentation and editing.

Business rules remain in the Domain and Application layers; these components only manage presentation and interaction.
## Inventory interaction reliability

The Inventory page uses an explicit interaction state to prevent concurrent mutations and duplicate submissions. Transaction and tag forms remain open after failed saves so user input is preserved. Destructive actions require confirmation, Escape closes idle dialogs, controls are disabled while requests are running, and server exceptions are translated into stable user-facing messages.

Inventory data is refreshed after successful mutations. The UI deliberately avoids speculative balance, transaction, or tag updates because those values depend on server-side validation and aggregate calculations; existing data remains visible if a refresh fails.


## Inventory search and filters

Inventory transactions can be filtered in memory by description or notes, transaction type, tag, and inclusive date range. Results can be ordered by transaction date in ascending or descending order, and the UI exposes an active-filter count, one-action filter reset, pagination reset after filter changes, and a dedicated no-results state.

The Web layer only captures filter state. `GetTransactionsQuery` owns the filter contract so the same parameters can later be translated to paginated SQL predicates without redesigning the page components.


## Inventory responsive behavior

The Inventory module uses explicit desktop, tablet, and mobile breakpoints. Desktop keeps independent scrolling for long transaction and tag collections; tablet collapses the workspace while retaining compact summary cards; mobile converts filters, pagination, tag actions, and primary actions to full-width touch controls. Interactive controls use at least 44 px touch targets, visible keyboard focus, semantic labelled regions, and responsive modal constraints.


## Inventory UI test coverage

The Inventory module is covered with bUnit and state tests for:

- wallet summary rendering and currency formatting;
- transaction and tag form validation;
- search, type, tag, period, and sorting callbacks;
- disabled controls while an operation is running;
- initial empty state and filtered no-results state;
- transaction refresh and pagination loading states;
- filter reset, active-filter counting, and page reset;
- concurrent-operation prevention through `InventoryInteractionState`.

Tests intentionally assert public UI behavior and component contracts rather than internal CSS implementation details.
