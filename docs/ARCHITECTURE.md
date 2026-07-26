# Architecture

LevelUp follows a layered architecture with compile-time dependencies directed toward the Domain layer.

```text
LevelUp.Domain
    ↑
LevelUp.Application
    ↑
LevelUp.Infrastructure
    ↑
LevelUp.Web
```

`LevelUp.Web` is the composition root and registers Application and Infrastructure services.

## Project responsibilities

### LevelUp.Domain

Owns:

- entities and aggregate state;
- value objects and enums;
- domain events and exceptions;
- business invariants;
- character experience, progression curves, and reward history.

It must not depend on Application, Infrastructure, ASP.NET Core, or UI concerns.

### LevelUp.Application

Owns:

- commands, queries, requests, validators, and handlers;
- use-case orchestration;
- repository and technical-service contracts;
- MediatR pipeline behavior;
- the centralized automatic experience reward service.

It coordinates Domain behavior without knowing the concrete persistence or UI implementation.

### LevelUp.Infrastructure

Owns implementations for:

- JSON persistence and backup recovery;
- password hashing and identity support;
- email delivery;
- caching, auditing, clocks, queues, and hosted services;
- infrastructure health dependencies.

Infrastructure may depend on Application and Domain, but business rules must not be implemented here.

### LevelUp.Web

Owns:

- Blazor Server pages and components;
- feature-scoped UI state;
- authentication HTTP endpoints and cookies;
- middleware, diagnostics, and health endpoints;
- dependency-injection composition;
- presentation-only formatting and interaction behavior.

The Web layer must not access concrete JSON classes directly.

## Design rules

- Business invariants belong in Domain.
- Use-case sequencing belongs in Application.
- External systems and storage belong in Infrastructure.
- HTTP, component state, and visual behavior belong in Web.
- Cross-cutting behavior should use middleware, pipeline behavior, decorators, or shared abstractions.
- Persist only authoritative state; derive values that can be calculated reliably.
- Tests should assert public behavior and layer contracts rather than implementation details.

## Feature flow

A typical state-changing operation follows this path:

```text
Blazor component
    ↓
Web feature state/service
    ↓
Application command
    ↓
Application handler
    ↓
Domain entity/aggregate
    ↓
Repository contract
    ↓
JSON infrastructure implementation
```

## RPG progression boundary

`CharacterExperience.TotalExperience` and its transaction history are authoritative persisted state. Level, progress within the current level, required XP, and remaining XP are derived by `ExperienceCurve`.

Feature handlers may request an automatic reward through `IExperienceRewardService`, but they must not write XP fields directly. Idempotency is enforced by the source type, source identifier, and reward type recorded in the experience transaction history.
