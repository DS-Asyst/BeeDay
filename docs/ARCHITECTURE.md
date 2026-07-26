# Architecture

LevelUp follows a layered architecture with dependencies directed toward the Domain layer.

```text
LevelUp.Domain
    ↑
LevelUp.Application
    ↑
LevelUp.Infrastructure
    ↑
LevelUp.Web
```

## Projects

### Domain

Contains entities, value objects, enums, domain events, exceptions, and business invariants. It has no dependency on Application, Infrastructure, or Web.

### Application

Contains use cases, request handlers, validators, pipeline behaviors, and contracts required by infrastructure or presentation concerns. It coordinates the domain without depending on ASP.NET Core or a concrete persistence implementation.

### Infrastructure

Implements persistence, password hashing, email delivery, caching, auditing, background processing, clocks, health checks, and other technical services required by Application contracts.

### Web

Hosts the Blazor Server application, authentication endpoints, layouts, reusable UI components, feature pages, diagnostics middleware, and dependency injection composition.

## Design Rules

- Business rules belong in Domain.
- Use-case orchestration belongs in Application.
- External systems and storage belong in Infrastructure.
- UI state and HTTP concerns belong in Web.
- Web must access persistence through Application or declared contracts rather than concrete JSON classes.
- Cross-cutting behavior should use pipeline behaviors, middleware, decorators, or shared abstractions instead of duplicated feature code.
