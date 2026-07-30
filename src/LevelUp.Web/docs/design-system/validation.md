# Validation

Run from the full repository root:

```bash
dotnet format --verify-no-changes
dotnet build
dotnet test
git status
```

For visual changes also validate:

- Login and account flows.
- Create Character.
- Daily cards and modals.
- Inventory at desktop, tablet and mobile widths.
- Keyboard navigation and visible focus.
- 200% zoom.
- `prefers-reduced-motion: reduce`.
- Long labels and validation messages.

A ZIP containing only `LevelUp.Web` cannot compile independently because its project references Domain, Application and Infrastructure.
