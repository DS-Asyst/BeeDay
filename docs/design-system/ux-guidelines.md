# UX and Accessibility Guidelines

## Interaction

- Every action must have a clear default, hover, focus, active, disabled, and loading state when applicable.
- Prevent duplicate submissions while an asynchronous action is running.
- Destructive actions require explicit confirmation.
- Preserve user-entered values when validation fails.
- Empty states should explain the state and present the next valid action.

## Accessibility

- Use semantic HTML and real buttons for actions.
- Maintain visible keyboard focus.
- Provide accessible names for icon-only controls.
- Associate labels and validation messages with form fields.
- Do not rely on color alone to convey status.
- Respect reduced-motion preferences.
- Keep touch targets and spacing usable on smaller screens.

## Activity cards

- Keep title and description visually related.
- Maintain consistent bottom padding so counters, metadata, and action icons do not touch card edges.
- Place attribute and status metadata predictably across Habits, Tasks, To-Dos, and Projects.
- Preserve manual ordering unless the user explicitly selects another sorting mode.

## Card interaction contract (Habits, Tasks, To-Dos, Projects, Wallet Transactions, Wallet Tags)

- The entire card is the single entry point to Edit — no separate action-menu trigger, and no Edit/Delete buttons on the card itself (this replaced both the three-dot menu previously used by Wallet Transaction cards and the Edit/Delete buttons previously on Wallet Tag cards).
- Clicking or tapping anywhere on the card opens the item's existing edit dialog directly; there is no intermediate menu step.
- Internal controls nested in the card (completion checkbox, Habit +/− counters, and similar per-type controls) act independently: activating them never opens Edit (propagation is stopped where needed), and dragging the card never triggers them.
- Dragging a card (for manual reordering) and clicking it are mutually exclusive outcomes of the same gesture, distinguished by movement distance, not by card position or an explicit mode switch. Manual drag reordering applies to Habits, Tasks, To-Dos, and Projects; Wallet Transaction and Tag cards are not draggable.
- The clickable card body is keyboard-reachable, activates on Enter and Space, and carries an accessible name identifying the action and the item (for example, "Edit Habit: Morning run", "Edit Transaction: Rent, -$1,200.00", "Edit Tag: Groceries").
- Delete is not available directly from any of these cards (Habits, Tasks, To-Dos, Projects, Transactions, or Tags); it is reached through the edit dialog's existing confirmation flow for every entity type, without introducing a duplicate confirmation dialog.

## Static (non-interactive) cards

Informational cards that only display data — My Account's settings sections and Wallet's summary/panel cards (Current Balance, Total Income, Total Expenses, Transactions panel, Tags panel) — compose the Design System's non-interactive `LevelUpCard` primitive (or its underlying `.levelup-card` class) and carry no hover elevation, shadow bump, border change, or background transition. Interactive controls nested inside them (filters, pagination, New Tag/New Transaction actions) are unaffected and keep their own normal interactive states. Set `LevelUpCard`'s `Interactive="true"` only for a card that is itself a single clickable action — never to add decorative hover motion to an informational surface.

## Edit dialog actions

- Save stays pinned to the top-right of the dialog header and remains visible while the dialog content scrolls.
- Delete stays in the footer, left-aligned and visually separate from Save/Cancel, and always opens the existing delete-confirmation dialog rather than deleting immediately.
- Save and Cancel use their standard primary/secondary styling, including loading and disabled states, and guard against duplicate submission while a save is in progress.
