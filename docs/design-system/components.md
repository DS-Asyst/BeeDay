# Shared Components

Design System components are organized by responsibility under `Components/DesignSystem`.

## Groups

- **Activities**: shared activity presentation and controls;
- **Attributes**: semantic rendering for activity attributes;
- **Buttons**: standardized actions and icon integration;
- **Cards**: shared card surfaces and card behavior;
- **Feedback**: empty, loading, toast, warning, and validation states;
- **Forms**: labels, fields, selects, validation, checkboxes, and related controls;
- **Icons**: centralized pixel-icon renderer, contracts, registry, and catalog;
- **Layout**: reusable structural surfaces;
- **Modals**: shared dialogs and confirmation patterns;
- **Pages**: development-only visual review surfaces;
- **Text**: typography primitives.

## Usage rules

- Prefer composition over duplicating markup and CSS.
- Keep business decisions out of Design System components.
- Expose semantic parameters rather than physical asset paths.
- Preserve accessible names for interactive controls.
- Keep decorative icons hidden from assistive technologies.
- Avoid breaking public component parameters without updating all consumers and tests.
