---
name: beeday-ui-ux
description: BeeDay Experience System and Design System workflow for Blazor/Razor UI, CSS, components, icons, responsive behavior, accessibility, localization-visible behavior, interaction states, and visual consistency.
---

# beeday UI/UX and Experience System

Follow repository Experience System and Design System documentation before editing visual behavior.

## 1. Search before creating

Before adding UI, search for existing:

- foundations/tokens;
- layout primitives;
- typography roles;
- color roles;
- shared components;
- product patterns;
- form controls;
- dialogs/modals;
- loading/empty/error states;
- icon components and icon catalog;
- localization patterns;
- accessibility utilities.

Reuse an existing shared component when its contract fits. Extend it when the requirement is broadly reusable. Keep feature-specific behavior local when it is not a shared design concern.

## 2. Brand rules

Visible public brand:

- name: `beeday`;
- casing: lowercase;
- official brand color: `#5247F9`.

Do not infer a technical rename from a visual brand change.

## 3. Interaction-state matrix

For each interactive component, evaluate applicable states:

- default;
- hover;
- focus-visible;
- active/pressed;
- selected;
- disabled;
- loading;
- validation;
- success;
- warning;
- error;
- empty.

Do not ship a component that only works in the happy-path visual state.

## 4. Accessibility gate

Verify applicable requirements:

- semantic element choice;
- keyboard navigation;
- visible focus;
- accessible names;
- labels and descriptions;
- `aria-*` usage only where semantically necessary;
- alt text for meaningful imagery;
- decorative imagery hidden appropriately;
- sufficient state communication without color alone;
- dialog focus behavior;
- disabled/loading semantics.

Do not trade accessibility for visual similarity.

## 5. Responsive and content-resilience gate

Validate relevant breakpoints and layouts for:

- narrow mobile width;
- tablet/intermediate width;
- desktop width;
- long localized labels;
- multiline titles;
- empty content;
- large dynamic values;
- reduced motion when motion exists.

Avoid fixed dimensions that break known responsive patterns unless the Design System explicitly defines them.

## 6. Localization

Visible strings must follow the repository localization architecture.

Do not reintroduce hard-coded user-visible English or Portuguese when the feature is localized.

Preserve supported culture behavior and persistence rules defined by the repository.

## 7. Icons and assets

Use the repository icon system and documented asset process.

Do not introduce an icon library, raw external icon, or custom visual primitive when the repository already defines the equivalent.

## 8. UI test strategy

Update the appropriate combination of:

- component/bUnit tests;
- integration tests;
- accessibility assertions;
- Playwright/E2E tests;
- selector expectations;
- manual runtime verification when visual behavior cannot be proven automatically.

Selectors should describe stable product behavior rather than incidental DOM structure whenever repository conventions allow it.
