# AI Architecture — LevelUp

## Dependency direction

```text
Domain ← Application ← Infrastructure ← Web
```

Web is the composition root and also references Domain and Application directly where required for UI contracts and composition. This does not transfer business ownership to Web.

## Decision table

| Concern | Owning layer |
| --- | --- |
| Entity invariants and state transitions | Domain |
| Value validation intrinsic to the domain | Domain |
| Commands, queries, requests, and handlers | Application |
| Use-case validation and orchestration | Application |
| Repository and technical-service contracts | Application |
| JSON persistence and backups | Infrastructure |
| Password hashing and external email delivery | Infrastructure |
| Caching, auditing, clocks, queues, and hosted workers | Infrastructure |
| HTTP endpoints, cookies, antiforgery, middleware | Web |
| Blazor components, UI state, CSS, and static assets | Web |
| Shared visual primitives | Web Design System |

## Typical mutation

```text
Component → Web service/state → Command → Handler → Domain → Repository contract → JSON implementation
```

## Experience boundary

Persist total XP and transaction history. Calculate level and progress through the curve. Grant automatic rewards only through the centralized Application experience service so idempotency and events remain consistent.

## UI boundary

Feature components may compose shared Design System components. They must not bypass the Pixel Icon renderer with direct SVG paths or implement business rules that belong in Domain/Application.

## Persistence boundary

The repository contract is the only supported application persistence boundary. Do not couple feature handlers or Web components to concrete JSON files. A future database migration should replace Infrastructure implementations without rewriting domain rules.
