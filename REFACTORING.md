# Backend refactoring applied

## Structure
- Domain entities and enums split into individual files.
- Application service split into feature files for profile, habits, tasks, to-dos, and projects.
- Request records replace long primitive parameter lists.
- Application and Infrastructure dependency injection registrations are separated.
- Generated Visual Studio and build artifacts were removed.
- Root `.gitignore`, `.editorconfig`, and `Directory.Build.props` were added.

## Robustness
- Domain update methods validate titles and descriptions.
- Application-specific not-found exception added.
- JSON persistence retains atomic writes and adds rotating timestamped backups.
- Storage directory is constrained to the application content root.
- `/health` now validates that JSON storage can be loaded.

## Tests
- Domain, Application, and Infrastructure test projects were added under `tests/`.
- Initial tests cover habit counters, title validation, and duplicate identifiers.

## Compatibility
- Existing JSON schema, namespaces used by the UI, routes, components, styling, and user data were preserved.
