
# Claude Code Instructions — LevelUp

This file defines the permanent operating rules for Claude Code in this repository.
The detailed source of truth is under `docs/`.

---

## Required reading

Before changing code, read in this order:

1. `docs/README.md`
2. `docs/architecture/01-current-state.md` and `docs/architecture/02-target-architecture.md`
3. `docs/contracts/01-contract-first-standard.md`
4. `docs/data/03-json-to-sql-transition.md`
5. Documentation related to the affected feature/layer.

---

# Core Principles

- Inspect the existing implementation before proposing changes.
- Verify assumptions in the code.
- Prefer evidence over documentation when they disagree.
- Preserve architecture before adding features.
- Prefer incremental refactoring.
- Never introduce breaking changes without approval.

---

# Architecture Contracts

These rules are permanent repository contracts.

- Contract-First is mandatory.
- Contracts are the source of truth.
- Domain contains business rules.
- Application orchestrates use cases.
- Infrastructure contains technical implementations.
- Web contains presentation only.

Never expose:

- Domain entities as API contracts.
- EF Core entities outside Infrastructure.
- Infrastructure models inside Web.

---

# Dependency Rules

Allowed:

Web
↓
Contracts
↓
Application
↓
Domain
↑
Infrastructure

Forbidden:

- Web -> DbContext
- Application -> DbContext
- Application -> JSON implementation
- Application -> SQL implementation
- Domain -> ASP.NET
- Domain -> EF Core
- Contracts -> Infrastructure

---

# Persistence Rules

JSON is temporary.

- No new feature may depend on JSON.
- JSON exists only as a temporary adapter.
- SQL Server is the future persistence.
- Repositories are contracts.
- Infrastructure provides implementations.

---

# Repository Rules

Avoid generic repositories.

Prefer:

- IUserRepository
- IHabitRepository
- IProjectRepository
- ITodoRepository
- IWalletRepository

Repositories represent aggregates, not tables.

---

# DTO Rules

Never reuse the same class across layers.

Contracts != Commands != Queries != Responses != Domain != EF Models != View Models

---

# EF Core Rules

EF Core is Infrastructure only.

DbContext must never be referenced from:

- Web
- Application
- Domain

---

# Database Policy

The SQL database starts empty.

- No JSON migration.
- No legacy import.
- No compatibility layer after SQL becomes primary.

---

# Working Principles

- Preserve layered architecture.
- Reuse the Design System.
- Update tests whenever behavior changes.
- Update documentation in the same PR.
- `.editorconfig`, `Directory.Build.props` and `Directory.Packages.props` are repository contracts.

---

# Safety and Git

- Never rewrite history.
- Never force-push.
- Never deploy without approval.
- Never commit secrets, backups, generated data or local configuration.
- Show git diff and validation before proposing commits.
- One architectural concern per change.

---

# Technical Debt

When technical debt is found:

- Report it.
- Explain impact.
- Suggest solution.
- Wait for approval.

Do not silently refactor unrelated code.

---

# Mandatory Validation

Run:

```bash
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx --configuration Release --warnaserror
dotnet test LevelUp.slnx --configuration Release
git status
```

If UI changes:

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

---

# Definition of Done

A task is complete only when:

- Architecture respected
- Contracts updated
- Tests updated
- Documentation updated
- Formatting passes
- Build succeeds
- Tests pass
- No warnings
- Git status clean (unless requested otherwise)

---

# AI Behavior

Always:

- Inspect
- Verify
- Search
- Explain trade-offs

Never guess implementation details.

If code and documentation disagree:

Follow the code and report the documentation mismatch.
