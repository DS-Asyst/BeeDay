# Delivery F.1 — Drag & Drop

## Scope

Sprint F.1 adds persistent card reordering to each dashboard collection:

- Habits
- Tasks
- To-Dos
- Projects

The order is stored in `LevelUpBD.json`, so it remains stable after refresh and application restart.

## Interaction

- Mouse: hold the drag handle and move the card.
- Touch or pen: press the drag handle and move the floating preview.
- Keyboard: focus the handle and use `ArrowUp` or `ArrowDown`.
- A visible insertion line identifies the destination.
- Reduced-motion preferences disable reorder transitions.

## Persistence design

The Application layer exposes `IActivityOrderService`. The request contains the collection and the ordered identifiers currently visible on the dashboard.

`LevelUpData` reorders only the submitted identifiers and preserves the positions of filtered-out cards. This allows reordering while a dashboard search is active without losing hidden items.

## Compatibility rule

Cards are reordered only inside their own collection. Moving a Habit into Tasks, or converting a Task into a To-Do, is intentionally not supported because those entities have different domain rules and editor models.

## Files

- `Components/Behaviors/DragDrop/LevelUpSortable.*`
- `Components/Behaviors/DragDrop/SortableOrder.cs`
- `wwwroot/js/levelup-sortable.js`
- `wwwroot/css/dragdrop.css`
- `Features/Ordering/*`

## Validation

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Manual validation:

1. Reorder two cards with the mouse and refresh the browser.
2. Repeat with touch or pen, when available.
3. Focus a drag handle and use the arrow keys.
4. Search for a subset of cards, reorder them, clear the search and confirm hidden cards were preserved.

## F.1.1 — Card-surface reordering

The visible drag handle was removed. Users now reorder an item by holding and dragging the card surface itself. Interactive controls inside the card (buttons, links and form fields) are excluded from drag activation. Touch and pen use a short long-press threshold, while keyboard users can focus the card and press Arrow Up or Arrow Down.
