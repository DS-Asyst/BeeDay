# Typography

## Families

| Family | Responsibility |
|---|---|
| Inter | body copy, forms, labels, tables, messages and card content |
| Jersey 25 | brand, navigation, page/section identity and standard actions |
| Press Start 2P | small metrics, XP, counters, achievements and deliberately pixel-styled actions |

## Semantic roles

Use roles rather than direct families:

- `--levelup-type-display`
- `--levelup-type-page-title`
- `--levelup-type-section-title`
- `--levelup-type-card-title`
- `--levelup-type-body`
- `--levelup-type-body-small`
- `--levelup-type-label`
- `--levelup-type-button`
- `--levelup-type-button-body`
- `--levelup-type-button-pixel`
- `--levelup-type-metric`

Utility classes mirror the primary text roles. Prefer component parameters when a component exposes typography.

## Prohibited patterns

- `font-family: "Inter"` inside a feature stylesheet.
- global `button { font-family: ... !important; }`.
- using Press Start 2P for paragraphs or long labels.
- changing font family to compensate for incorrect font size or spacing.
