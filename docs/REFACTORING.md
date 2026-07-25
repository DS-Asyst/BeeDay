# Refactoring Guidance

## Review outcome

The current layered architecture is appropriate for the size and direction of LevelUp. A rewrite or additional project layer would increase ceremony without solving a demonstrated problem. Improvements should continue through focused extraction inside the existing boundaries.

## Completed in this review

- Removed the unused duplicate `ProfileSummaryCard` Razor component and retained `CharacterSummaryCard` as the single dashboard summary implementation.
- Removed sprint-specific documentation and the obsolete frontend-only README.
- Consolidated the official typography to Jersey 15 and Pixelify Sans.
- Replaced direct font declarations in Inventory styling with design-system font variables.
- Added a maintained project map and consolidated refactoring guidance.

## Priority refactoring backlog

### 1. Replace global current-user persistence

`LevelUpData.CurrentUserId` makes the persisted aggregate responsible for session context. Before multi-user deployment, resolve the current user from authentication claims or a scoped session abstraction. Repository operations should receive or derive a user identifier without mutating global shared state.

### 2. Decompose `LevelUpData` by policy

Keep `LevelUpData` as the persisted root for JSON storage, but extract cohesive policies when changes require them:

- schema migration service;
- ownership/identity validation policy;
- activity ordering policy;
- uniqueness policy for users, nicknames and inventory tags.

Do not split entities into separate repositories while JSON remains a single atomic document unless consistency rules are redesigned.

### 3. Thin large Razor pages

`Inventory.razor` and dashboard `Home.razor` coordinate loading, commands, modal state and rendering. Move orchestration into page-specific state/facade classes and keep Razor files focused on markup and event delegation. Components should not access persistence contracts directly.

### 4. Separate inventory read models

Inventory query handlers should return purpose-built immutable projections for summary, transaction list and tags. Avoid making the UI reconstruct wallet totals or tag metadata. This also prepares the feature for database-backed queries later.

### 5. Formalize localization

The interface is consistently English, but strings are embedded in Razor. When a second language is implemented, introduce `IStringLocalizer` resources feature by feature, beginning with navigation, authentication, account and shared validation. Avoid a mixed state where only isolated controls are localized.

### 6. Strengthen authorization

Every command/query that changes or reads user-owned data should validate ownership from trusted execution context. Identifiers submitted by the UI are not authorization evidence.

### 7. Keep application handlers narrow

Handlers should orchestrate validation, repository access and domain behavior. Extract services only when logic is reused or constitutes a standalone policy. Avoid generic service classes that merely forward repository calls.

### 8. Prepare persistence evolution

The Application repository contract is the migration seam. A future relational provider should introduce transactions, optimistic concurrency and migrations without exposing EF Core types to Domain or Application.

## Decentralization rules

- Domain rules remain in entities/value objects, not Razor or handlers.
- Use Application behaviors for cross-cutting request concerns.
- Infrastructure owns filesystem, hashing, caching and hosted processing.
- Web owns HTTP/session context and presentation state.
- Shared visual rules belong to the design system; feature CSS owns only feature-specific layout and presentation.
- Avoid static mutable state and singleton services containing user-specific data.

## Change safety checklist

1. Add or update tests for the behavior before extraction.
2. Move one cohesive responsibility at a time.
3. Preserve public contracts unless migration is intentional.
4. Run formatting, Release build and all tests.
5. Validate JSON migration and backup recovery against a copy of real data.
6. Review authorization and user ownership for every new operation.
