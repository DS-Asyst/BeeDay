# AI Workflow — LevelUp

## 1. Establish state

- Confirm the repository root and branch.
- Run or inspect `git status` when Git metadata is available.
- Read `CLAUDE.md`, all files under `docs/ai/`, and the relevant domain/architecture/design documents.
- Inspect the actual affected files and tests; do not rely only on task wording.

## 2. Plan

- Restate the exact scope internally.
- Identify affected layers, contracts, tests, documentation, and migration/compatibility concerns.
- Prefer the smallest complete implementation.
- Surface an architectural ambiguity before choosing a disruptive direction.

## 3. Implement

- Follow existing naming and directory conventions.
- Preserve unrelated code and formatting.
- Keep business rules in their owning layer.
- Add tests alongside behavior changes.
- Update maintained documentation, not historical sprint files.

## 4. Review twice

First review:

- inspect every changed file;
- check correctness, nullability, validation, cancellation, user isolation, accessibility, and unintended scope.

Second review:

- compare the final diff against the task;
- verify architecture, documentation consistency, line endings, generated/binary assets, and test coverage;
- search for obsolete references left by renames or removals.

## 5. Validate

```bash
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
git status
```

Use Release validation for CI/deployment-sensitive work. Run the Web project for manual UI verification when required.

## 6. Handoff

Report:

- what changed;
- key architectural decisions;
- files or areas affected;
- tests and commands executed with exact results;
- remaining risks or manual checks;
- proposed commit message only when useful.

Do not commit or push unless explicitly requested.
