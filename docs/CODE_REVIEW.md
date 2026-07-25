# Code Review

Review baseline: source package `LevelUp(73)` reviewed on 24 July 2026.

## Scope

The review covered solution metadata, project references, domain entities and value objects, application features, infrastructure services, Blazor components, configuration, scripts, workflows and tests. It also included searches for obsolete code, duplicated files, generated artifacts, runtime data and common exception/TODO markers.

A full compile and test execution could not be performed in the review environment because the .NET SDK was unavailable. The repository CI remains the authoritative build validation step.

## Corrections applied

### Removed obsolete sprint documentation

The root-level sprint files were implementation snapshots rather than durable project documentation. They were removed and replaced with maintained architecture, development and review documents.

### Removed dead Profile implementation

The repository had two competing models:

- Current model: `User` + `Character`
- Legacy model: `Profile`

Legacy Profile files still existed in Domain, Application and Web, while project files explicitly excluded them from compilation/content. This was fragile because it hid dead code rather than deleting it. The obsolete entity, value objects, application feature, UI feature and cleanup script were removed. The private legacy JSON snapshot in `LevelUpData` was retained because it performs persisted-data migration and is not part of the obsolete public model.

### Removed generated and machine-specific artifacts

The source package included `.git`, `.vs` and a complete `publish` output. These inflate archives, leak local repository/editor state and can become stale relative to source. They were removed from the reviewed distribution.

### Removed runtime account data

`src/LevelUp.Web/Data` contained a real e-mail address, a password hash, event journal entries and backups. The directory was reset to `.gitkeep`. Runtime files are already excluded by `.gitignore` and must not be committed or shared.

### Corrected solution metadata

`LevelUp.slnx` referenced nonexistent historical files (`REFACTORING.md` and `STAGE-1.md` through `STAGE-4.md`). The solution items now reference the actual maintained documentation.

### Simplified project files

Compile/content exclusion rules used solely to suppress obsolete Profile files were removed after the underlying dead files were deleted.

## Architecture findings

### Strengths

- Clear project dependency direction
- Domain project free of external dependencies
- Central package version management
- Nullable reference types and analyzers enabled centrally
- Feature-oriented application and UI organization
- Strong domain validation around ownership and identifiers
- Persistence decomposed into focused services
- Atomic/recoverable local storage design
- Health checks and structured logging
- CI and production validation before deployment
- Broad test coverage across all four projects
- Reusable Blazor design-system components

### Important risks

#### 1. Global current-user state

`LevelUpData.CurrentUserId` is persisted as part of a single shared JSON aggregate. This is acceptable for a local single-user application, but unsafe for a concurrent multi-user web deployment. Authentication work must ensure that user context comes from the authenticated session rather than globally mutating one persisted current user.

**Priority:** Critical before enabling real multi-user access.

#### 2. Authentication boundary requires hardening

Cookie authentication, login and logout flows are present, and password hashing is isolated behind `IPasswordService`. The remaining risk is the persisted global `CurrentUserId`: authenticated identity must become the sole source of user context before concurrent multi-user deployment. Account recovery, lockout/rate limiting and production cookie policy should also be completed.

**Priority:** High before public deployment.

#### 3. Domain aggregate growth

`LevelUpData` currently combines schema migration, aggregate validation, ownership assignment, search and ordering. This is manageable at the current size but will become a maintenance hotspot as inventory, books, finance or achievements are added.

**Recommendation:** Extract migrations and collection-specific policies when the aggregate grows further; do not split prematurely while behavior remains cohesive.

#### 4. Read-model efficiency

`Todos` is calculated by flattening all project To-Dos into a new list. This is correct but allocates on every access. For the present data scale it is negligible. For large data sets, use projections at query time or expose an enumerable/read-only view.

**Priority:** Low.

#### 5. Background work durability

The background queue is in-memory. Queued operations are lost on process restart. This is acceptable for noncritical cache/audit work but unsuitable for business-critical jobs.

**Priority:** Medium only when durable jobs are introduced.

#### 6. Deployment coupling

The production workflow targets a specifically labeled self-hosted Windows runner and IIS script. This is practical for the homelab but should be documented and protected with GitHub environments, restricted runner access and repository/environment secrets.

**Priority:** Medium.

## Duplication findings

- Exact duplicate runtime backup files were found under `Data/Backups`; all runtime backups were removed from the source package.
- The main semantic duplication was the obsolete Profile domain/application/UI stack alongside User + Character; it was removed.
- No duplicate fully-qualified active C# type declarations were identified in the maintained source set.

## Test observations

The source includes four test projects and approximately 79 `[Fact]`/`[Theory]` declarations at review time. Coverage includes:

- Value objects and domain rules
- Habit, project and ordering behavior
- Application requests, handlers and domain events
- JSON persistence and recovery behavior
- Correlation middleware
- Buttons, cards, forms, feedback components and text highlighting

Recommended additions:

- User/character uniqueness and migration edge cases
- Concurrent repository save tests
- Authentication and authorization tests when implemented
- End-to-end onboarding to Daily navigation
- Settings persistence tests
- Production configuration validation

## Recommended next actions

1. Run `dotnet restore`, `dotnet build -c Release` and `dotnet test -c Release` locally or through CI.
2. Implement authentication with per-session user context before exposing multiple accounts.
3. Add explicit authorization checks to every user-owned command/query.
4. Move password hashing behind a dedicated application/security abstraction if it is not already isolated.
5. Add secret scanning and dependency/security scanning to CI.
6. Protect the `prd` environment and self-hosted deployment runner.
7. Keep roadmap/sprint planning outside the repository root or under a clearly archival project-management location.

## Corrections after the review

- The legacy `Application/Features/Profiles` module was permanently removed.
- There are no remaining references to `ProfileCommandHandlers`, `LevelUpData.Profile` or `SetProfile`.
- The account was consolidated into the `User` and `Character` aggregates.
- The 33 `IDE0011` occurrences reported in the July 24, 2026 build were fixed with explicit braces in control structures.

### Recommended local validation

```bash
dotnet clean
dotnet build
dotnet test
```

The environment used to prepare this package does not include the .NET SDK, so compiled validation must be performed on a workstation with .NET 10 installed.
