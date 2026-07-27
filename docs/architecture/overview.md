# System Overview

## Runtime shape

LevelUp is a server-rendered interactive Blazor application hosted by ASP.NET Core. The Web project configures authentication, authorization, antiforgery, exception handling, health checks, static assets, Razor Components, Application services, and Infrastructure implementations.

A typical state-changing request follows:

```text
Blazor component
    ↓
feature state or LevelUpWebService
    ↓
MediatR command
    ↓
Application handler and validator
    ↓
Domain entity or aggregate
    ↓
ILevelUpRepository
    ↓
JSON infrastructure implementation
```

## Cross-cutting capabilities

- FluentValidation request validation through an Application pipeline behavior;
- MediatR command/query dispatch and domain-event notifications;
- cookie authentication and persisted active-user validation;
- structured logging, correlation identifiers, and centralized exception handling;
- in-memory caching and identity-request throttling;
- background task queue and hosted worker;
- event journal auditing;
- health checks for liveness, readiness, and storage;
- centralized Design System and Pixel Icon registry.

## Authoritative boundaries

The JSON repository persists the authoritative application state. Derived values such as character level and progress are calculated from persisted total experience and the configured experience curve.

The UI does not write storage directly. It dispatches use cases through Application contracts or Web feature services.
