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

<<<<<<< HEAD
## 7. Sprint Completion Protocol

When work is organized into explicit Sprints:

- Stop implementation immediately once the Sprint scope has been completed.
- Do not start additional improvements.
- Do not suggest extra work.
- Wait for explicit user validation.

A Sprint is considered complete only when the user replies:

`VALIDADO`

After `VALIDADO`:

1. Commit the completed Sprint before starting any new work.
2. By default, create a single Conventional Commit representing the Sprint.
3. Multiple commits are allowed only if explicitly requested by the user.
4. Verify the repository is clean using:

   git status

5. If the working tree contains changes from multiple Sprints, stop and ask the user how to proceed. Never reconstruct Sprint history automatically.
6. Only after a clean working tree is confirmed, clear the Sprint implementation context.
7. Wait for the user to start a new conversation before beginning the next Sprint.

Do not carry implementation context from a completed Sprint into future work.
=======
## 7. Sprint completion protocol

When work is organized into an explicit Sprint:

- Stop implementation as soon as the Sprint's stated scope is delivered and validated. Do not start additional improvements or suggest further changes beyond that scope.
- Wait for explicit user validation before treating the Sprint as complete. The literal reply `VALIDADO` is the sole signal that a Sprint is approved and finished — no other confirmation implies it.
- After `VALIDADO`, do not carry that Sprint's implementation context into further work. Wait for the user to start a new conversation before beginning the next Sprint.
>>>>>>> hmg
