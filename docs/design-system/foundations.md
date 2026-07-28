# Visual Foundations

LevelUp uses a pixel-inspired visual identity while retaining modern accessibility and responsive behavior.

## Foundations

- consistent spacing and card padding;
- centralized typography roles;
- semantic success, warning, danger, neutral, and accent states;
- predictable hover, focus, disabled, loading, and selected behavior;
- reusable modal, form, feedback, card, button, text, layout, attribute, and activity components;
- responsive layouts that avoid coupling functionality to a single viewport size.

## Source of truth

Component CSS, global CSS, and shared component APIs are authoritative. Do not copy a visual value from a screenshot when an existing token, component, or class already expresses the intended contract.

## Change policy

Visual changes must be reviewed across:

- normal and compact viewport widths;
- keyboard focus and tab order;
- hover and disabled states;
- loading, empty, warning, validation, and error states;
- text wrapping and long localized content;
- reduced-motion preferences where animation is used.

## Pixel adapter (NES.css-derived, restricted use)

`wwwroot/css/pixel-nes.css` is an internal Design System adapter providing two LevelUp-owned classes, `.levelup-pixel-panel` and `.levelup-pixel-cta`, for a genuine "special pixel experience" surface — not general operational UI. It is a curated, LevelUp-recolored subset of NES.css 2.3.0 (MIT License; full provenance, included/excluded upstream selectors, and every adaptation decision recorded in `wwwroot/css/vendor/NES_ATTRIBUTION.md`). No `.nes-*` class name is used anywhere in the application; the adapter reproduces only the border-image pixel-corner mechanic, recolored with existing LevelUp tokens.

**Current consumer**: the Level Up celebration modal (`LevelUpFeedbackModal`) — its panel and its "Continue" CTA. This is the only approved use.

**These classes are not general-purpose styling hooks.** Applying either to a new consumer requires, in this order:

1. A genuine, one-off special pixel experience — not an ordinary operational action (Save, Cancel, Delete, Edit, filtering, navigation, forms, or any other routine UI stays on the modern Design System components: `LevelUpButton`, `LevelUpConfirmDialog`, `EditorModalShell`, `ActivityAttributeBadge`, `LevelUpCard`, unchanged);
2. Design System review of the proposed consumer;
3. Accessibility validation (keyboard operation, focus visibility, contrast, 200% text zoom, reduced motion);
4. Explicit written confirmation that the action is not routine/operational.

The adapter is purely additive: every consumer's existing modern styling (radius, shadow, `LevelUpButton` variant behavior) remains in place underneath it, so a failure to load `pixel-nes.css` degrades to the fully functional modern appearance, never to unstyled or broken markup.
