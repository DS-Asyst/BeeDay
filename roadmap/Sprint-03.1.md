# Sprint 3.1 — Dashboard Foundation

**Status:** Implemented — local build validation required

## Goal

Replace the design-system showcase with the first official LevelUp dashboard.

## Deliverables

- [x] Top navigation
- [x] Active navigation state
- [x] Gold balance preview
- [x] Dashboard header
- [x] XP progress
- [x] Filter bar
- [x] Create action placeholder
- [x] Habits column
- [x] Tasks column
- [x] To-Dos column
- [x] Projects column
- [x] Empty states
- [x] Desktop, tablet and mobile layouts
- [ ] Build validation (the delivery environment does not contain the .NET SDK)
- [ ] Test validation (the delivery environment does not contain the .NET SDK)

## Architecture notes

- Business rules remain outside Razor components.
- Dashboard preview values are exposed through `DashboardViewModel`.
- Feature-specific components are isolated under `Components/Dashboard`.
- Existing Git history and current branch are preserved.
