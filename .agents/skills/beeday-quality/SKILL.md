---
name: beeday-quality
description: Mandatory BeeDay test planning and validation workflow. Use after every implementation and whenever build, test, release validation, EF consistency, workflow validation, or failure classification is required.
---

# BeeDay Quality Gate

This Skill defines the evidence required before an implementation can be called complete.

## 1. Test selection

Select tests according to the changed contract.

Consider:

- Domain tests for invariants and business behavior;
- Application tests for commands, queries, handlers, validation, orchestration, cancellation, and policies;
- Infrastructure tests for provider behavior, persistence, transactions, constraints, integrations, and migrations;
- Web/bUnit tests for presentation behavior and localized interaction;
- integration tests for cross-layer behavior;
- architecture tests for dependency and forbidden-reference rules;
- E2E/Playwright for critical user flows when applicable.

Do not run only the test project that contains the changed file if the behavior crosses boundaries.

## 2. Mandatory baseline validation

From the repository root run the repository-canonical equivalent of:

```bash
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx
dotnet test BeeDay.slnx
git status
```

Run when applicable:

```bash
git diff --check
```

Use repository-documented Release validation for release-sensitive or infrastructure-sensitive changes.

## 3. Specialized validation

### PowerShell

Parse every modified `.ps1` with the PowerShell parser or repository-supported equivalent.

### GitHub Actions / YAML

Validate YAML/workflow syntax through the repository-supported mechanism when available.

### Entity Framework Core

When model/schema consistency is relevant, run the repository-documented pending-model-changes check with the correct project/startup-project pairing.

### UI

Use automated Web/bUnit/E2E coverage and repository-documented runtime verification when interaction/visual behavior cannot be proven by tests alone.

### Infrastructure

Distinguish local simulation from actual environment execution. Local parsing/build/test evidence is not IIS, deployment, certificate, Scheduled Task, or production evidence.

## 4. Failure handling

Never hide the first failure.

Classify every failure:

- `CHANGE-CAUSED`;
- `PRE-EXISTING`;
- `ENVIRONMENT`;
- `TRANSIENT/FLAKY`;
- `UNCLASSIFIED`.

A classification requires evidence.

If a retry passes, record both the original failure and the retry result.

Do not call a failure flaky simply because it disappeared.

## 5. Completion gate

The quality gate passes only when:

- all mandatory commands ran;
- no unresolved change-caused build/test/format failure remains;
- relevant specialized checks ran;
- test coverage matches changed behavior;
- validation output is reported accurately.

Do not claim success based on expected results.
