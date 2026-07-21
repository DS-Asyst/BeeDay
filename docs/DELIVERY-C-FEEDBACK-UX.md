# Delivery C — Feedback & UX

This delivery introduces a reusable feedback layer for the LevelUp web application.

## Added components

- `LevelUpToastHost`: success, error, and informational notifications.
- `LevelUpLoading`: blocking operation feedback while persistence is running.
- `LevelUpSkeleton`: reusable loading placeholder.
- `LevelUpDashboardSkeleton`: dashboard-specific loading composition.
- `LevelUpConfirmDialog`: reusable destructive-action confirmation dialog.
- `ToastService`: scoped notification coordinator.

## Applied workflows

- Create and update Habit, Task, To-Do, and Project.
- Delete from cards and editor modals.
- Persistence failure feedback.
- Dashboard initial loading skeleton.
- Character creation feedback.

## Behavior

- Successful saves and deletions produce a toast.
- Errors keep the editor state available whenever possible and produce an error toast.
- Concurrent operations are blocked by `IsBusy`.
- Destructive confirmation cannot be dismissed while its operation is running.
- Existing domain and persistence behavior remains unchanged.


## Compatibility correction C.1

- Corrected interpolated attributes in `LevelUpConfirmDialog` usage on `Home.razor`.
- Removed the obsolete `LevelUp.Web.Components.Shared` import after migrating the delete dialog to the Design System.


## C.2 — Loading refinement

- Removed the full-screen loading veil from routine dashboard operations.
- Loading feedback is now compact and non-blocking.
- A 350 ms reveal delay prevents flicker during fast JSON persistence operations.
- The indicator remains available for operations that take long enough to require feedback.
