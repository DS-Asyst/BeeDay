# Forms

Canonical controls are `LevelUpInput`, `LevelUpDateInput`, `LevelUpSelect`, `LevelUpTextArea`, `LevelUpCheckbox` and `LevelUpValidationMessage`.

## Contract

- Labels use `--levelup-type-label`.
- Entered values use `--levelup-type-body`.
- All controls preserve visible focus, disabled and readonly states.
- Validation text uses the semantic danger token and remains programmatically associated with its control.
- Authentication POST forms may use native inputs when required by endpoint binding, but must reuse `.auth-field` and semantic tokens.

## Page CSS responsibility

Page CSS may arrange fields into grids and set widths. It must not redefine the global field font, focus ring or validation palette.
