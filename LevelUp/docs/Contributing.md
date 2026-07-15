# Contributing to LevelUp

## Requirements

- .NET 10 SDK;
- Git;
- a modern terminal with Unicode support;
- an editor or IDE with C# support.

## Local Setup

```bash
git clone <repository-url>
cd LevelUp
dotnet restore
dotnet build
dotnet run --project LevelUp/LevelUp.csproj
```

## Branching

- `main`: stable completed phases;
- `develop`: integrated development baseline;
- `feature/<phase>-<feature>`: isolated feature work.

Create feature branches from `develop`. Merge completed and validated feature work back into `develop`. Merge `develop` into `main` when a product phase is complete.

## Validation

Before committing:

```bash
dotnet format
dotnet build
```

Manually validate the affected workflow and persistence behavior. Do not commit `Data/save.json`, `bin`, `obj` or IDE-specific files.

## Architecture Rules

1. Domain types must not depend on Spectre.Console.
2. Business rules belong in Domain entities or feature services.
3. Screens coordinate workflows; Components render information.
4. All complete game-state persistence goes through `GameStateService`.
5. Plain text and Spectre.Console markup must be handled explicitly.
6. Documentation must be updated with architecture or terminology changes.

## Commit Style

Use concise Conventional Commit messages where practical:

```text
feat: complete project and quest workflows
refactor: introduce shared entity card
fix: preserve project data during save
 docs: close phase 3 roadmap
```
