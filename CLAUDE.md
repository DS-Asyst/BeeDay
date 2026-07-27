# Claude Code Instructions — LevelUp

This file defines the permanent operating rules for Claude Code in this repository. The detailed source of truth is under `docs/`.

## Required reading

Before changing code, read:

1. `docs/ai/AI_CONTEXT.md`
2. `docs/ai/AI_RULES.md`
3. `docs/ai/AI_ARCHITECTURE.md`
4. `docs/ai/AI_WORKFLOW.md`
5. the documentation related to the affected layer or feature

## Working principles

- Inspect the existing implementation before proposing or applying changes.
- Preserve the current layered architecture and dependency direction.
- Prefer incremental changes over broad rewrites.
- Do not alter behavior outside the requested scope.
- Keep business rules in Domain, use-case orchestration in Application, technical implementations in Infrastructure, and presentation concerns in Web.
- Reuse the Design System; do not introduce isolated UI patterns when a shared component already exists.
- Add or update tests for changed behavior.
- Update the relevant documentation in the same change when architecture, behavior, configuration, or workflow changes.
- Treat `.editorconfig`, `.gitattributes`, `Directory.Build.props`, and `Directory.Packages.props` as repository contracts.

## Safety and Git

- Do not delete files in bulk, rewrite history, force-push, merge, rebase, publish, deploy, or remove branches without explicit approval.
- Do not commit secrets, runtime data, generated email files, logs, backups, build output, or local configuration.
- Show `git diff` and validation results before proposing a commit.
- Do not create a commit or push unless explicitly requested.

## Mandatory validation

Run from the repository root after implementation:

```bash
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
git status
```

For release-sensitive changes, also run:

```bash
dotnet build LevelUp.slnx --configuration Release --warnaserror
dotnet test LevelUp.slnx --configuration Release
```

When UI behavior must be manually verified:

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

A task is not complete when required validation fails. Report failures honestly and do not claim success without evidence.
