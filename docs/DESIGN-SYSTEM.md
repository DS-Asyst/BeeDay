# LevelUp Design System

## Objective

The Design System centralizes reusable presentation contracts without changing the current visual identity. Feature components consume these primitives instead of reproducing button, modal, feedback, and form markup.

## Current primitives

### Buttons

- `LevelUpButton`
- `LevelUpButtonVariant`
  - `Primary`
  - `Secondary`
  - `Danger`
  - `ConfirmationDanger`
  - `ConfirmationCancel`

### Forms

- `LevelUpInput`
- `LevelUpTextArea`
- `LevelUpSelect<TValue>`
- `LevelUpCheckbox`
- `LevelUpDateInput<TValue>`

The form components standardize labels, identifiers, validation messages, disabled states, CSS contracts, and Blazor `EditForm` integration. Feature models and validation rules remain inside their respective features.

`LevelUpTextArea` also supports an optional character counter. `LevelUpSelect<TValue>` and `LevelUpDateInput<TValue>` preserve strongly typed binding.

### Cards

- `LevelUpCard`
- `LevelUpCardMenu`

`LevelUpCard` is the semantic card shell used by dashboard cards. It centralizes the root `article` contract and supports contextual classes and additional HTML attributes.

`LevelUpCardMenu` owns the options trigger, open/close state, dismiss layer, Edit/Delete actions, accessible menu attributes, and automatic closing before action dispatch. Feature cards keep only their specific content and business events.

The reusable card styles are defined in `wwwroot/css/cards.css`. Task, To-Do, Project, and Habit visual rules remain distinct through modifier classes.

### Modals

- `EditorModalShell`

The shell owns backdrop behavior, accessible dialog attributes, form validation infrastructure, editor header actions, optional delete footer, and Escape handling.

### Feedback

- `LevelUpEmptyState`

Used for standardized empty collection messages. The component supports a message, optional icon, and contextual CSS class.

## Folder structure

```text
Components/DesignSystem/
├── Buttons/
├── Cards/
│   ├── LevelUpCard.razor
│   └── LevelUpCardMenu.razor
├── Feedback/
├── Forms/
│   ├── LevelUpCheckbox.razor
│   ├── LevelUpDateInput.razor
│   ├── LevelUpInput.razor
│   ├── LevelUpSelect.razor
│   └── LevelUpTextArea.razor
└── Modals/
```

## Form usage rules

1. Every input must receive a stable and unique `Id`.
2. Form primitives must be used inside an `EditForm` when validation is required.
3. Two-way binding must use `@bind-Value`, preserving the generated `ValueExpression` used by Blazor validation.
4. Feature-specific options and validation rules remain in feature components and models.
5. Components may receive contextual CSS classes, but must not contain feature-specific business rules.
6. New text, textarea, select, checkbox, and date fields should use the Design System primitives.

## General rules

1. Feature components must not duplicate editor backdrop, header, action, or delete footer markup.
2. New standard buttons should use `LevelUpButton`.
3. Domain and Application projects must never reference frontend Design System components.
4. Visual tokens remain under `wwwroot/css`.
5. Reusable form styles are defined in `wwwroot/css/forms.css`.
6. Dashboard cards must use `LevelUpCard` and `LevelUpCardMenu` instead of duplicating the root article and options menu markup.

## Next recommended primitives

- `LevelUpBadge`
- `LevelUpToast`
- `LevelUpLoading`
- `LevelUpSkeleton`

## Feedback

Delivery C adds `LevelUpToastHost`, `LevelUpLoading`, `LevelUpSkeleton`,
`LevelUpDashboardSkeleton`, and `LevelUpConfirmDialog`. Application workflows
publish notifications through the scoped `ToastService`.

## Delivery D — Theme & Tokens

The visual foundation is now expressed through semantic CSS tokens for color, spacing, typography, shape, elevation, focus, motion and layers. Components should prefer these tokens over literal values. See `DELIVERY-D-THEME-TOKENS.md`.
