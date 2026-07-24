# Architecture

## Overview

LevelUp follows a pragmatic Clean Architecture arrangement. Business concepts are isolated in the domain project, use cases are coordinated by the application project, technical services live in infrastructure, and the Blazor project acts as presentation layer and composition root.

## Dependency rules

| Project | May reference | Responsibility |
|---|---|---|
| `LevelUp.Domain` | Nothing | Business state, invariants, value objects and domain events |
| `LevelUp.Application` | Domain | Commands, queries, handlers, validators and ports |
| `LevelUp.Infrastructure` | Application, transitively Domain | Persistence, cache, audit, health checks and background processing |
| `LevelUp.Web` | Application, Domain, Infrastructure | UI, state containers, middleware and dependency composition |

Tests follow the same boundaries and reference only the layer they validate, except web tests, which exercise the presentation project and its transitive dependencies.

## Domain aggregate

`LevelUpData` is the current JSON aggregate root. It contains users, characters, habits, recurring tasks and projects. Project To-Dos are nested inside their owning project.

Important invariants enforced by the aggregate include:

- E-mail uniqueness
- One character per user
- Character nickname uniqueness
- A current user must exist before creating activities
- Every activity has an owner
- Every To-Do belongs to an existing project
- Entity identifiers remain unique
- Reordering rejects duplicate or unknown identifiers

The aggregate also includes migrations for legacy `profile` and top-level `todos` JSON fields. These private compatibility snapshots should remain until old persisted files no longer need to be supported.

## Application layer

The application layer is organized by feature:

```text
Features/<Feature>/
├── Commands or Queries
├── Handlers
├── Requests or Responses
└── Validation
```

MediatR dispatches commands and queries. Pipeline behaviors provide:

1. Structured logging
2. Performance diagnostics
3. FluentValidation execution
4. Domain-event publication

Repository access is abstracted by `ILevelUpRepository`. Handlers inherit common transaction-like load/mutate/save behavior through `RequestHandlerBase` where applicable.

## Infrastructure layer

### JSON repository

The persistence implementation separates path resolution, serialization, reading, writing and backup responsibilities:

- `JsonStoragePaths`
- `JsonSerializerOptionsFactory`
- `JsonFileReader`
- `JsonFileWriter`
- `JsonBackupService`
- `JsonLevelUpRepository`

Writes are serialized and protected against concurrent access. The repository validates loaded domain state and can recover from rotating backups when configured.

### Supporting services

- `MemoryApplicationCache` caches application projections.
- `JsonEventJournal` appends domain-event audit records.
- `BackgroundTaskQueue` and `BackgroundTaskWorker` execute queued work.
- `JsonStorageHealthCheck` verifies storage readiness.

## Web layer

The web project uses interactive server rendering. UI code is organized into:

- `Components/DesignSystem` for reusable primitives
- `Components/Features` for product functionality
- `Components/Layout` for shell and navigation
- `Services` for presentation-facing application access
- `Diagnostics` for middleware and exception handling
- `HealthChecks` for JSON health responses

Feature state classes are scoped to the Blazor circuit. `LevelUpWebService` acts as the UI facade over MediatR.

## Request flow

```text
Razor component
  -> Feature state / LevelUpWebService
  -> MediatR command or query
  -> Pipeline behaviors
  -> Handler
  -> ILevelUpRepository
  -> JsonLevelUpRepository
  -> LevelUpBD.json
```

After mutations, domain events can invalidate dashboard cache entries and append audit records.

## Data ownership

The current local repository supports multiple users in its schema, but the application operates on one `CurrentUserId` at a time. This is suitable for local development and migration work. A production multi-user deployment must scope repository reads/writes by authenticated identity and prevent one session from changing global current-user state.

## Evolution boundaries

The architecture supports these replacements with limited impact:

- JSON repository -> SQL/EF Core or cloud provider
- Blazor Server UI -> API plus SPA/client
- Memory cache -> distributed cache
- Local event journal -> centralized observability pipeline

Authentication is the most important missing architectural boundary. It should be introduced without placing password verification or session logic inside Razor components.
