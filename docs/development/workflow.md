# Git and Delivery Workflow

## Branches

- `hmg`: integration and validation
- `prd`: production
- temporary branches: `feature/*`, `fix/*`, `refactor/*`, `docs/*`, or `chore/*`

Start work from the current integration branch:

```bash
git switch hmg
git pull origin hmg
git switch -c feature/<scope>
```

## Implementation workflow

1. Confirm the working tree and branch.
2. Read the relevant documentation and implementation.
3. Define the exact scope and affected layers.
4. Implement the smallest complete change.
5. Add or update tests.
6. Update documentation when the contract changed.
7. Review `git diff` for unintended changes and line-ending noise.
8. Run the mandatory quality gate.
9. Commit only after validation.

## Mandatory quality gate

```bash
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
git status
```

For release-sensitive changes:

```bash
dotnet build LevelUp.slnx --configuration Release --warnaserror
dotnet test LevelUp.slnx --configuration Release
```

## Commit guidance

Use focused conventional-style messages, for example:

```text
feat(daily): add activity attribute filtering
fix(auth): reject unsafe return URLs
docs(ai): align agent workflow
refactor(inventory): isolate transaction mapping
```

Do not mix unrelated formatting, refactoring, behavior, and documentation changes unless they are inseparable.
