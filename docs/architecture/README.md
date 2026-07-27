# Architecture

LevelUp uses a layered architecture with compile-time dependencies directed toward Domain.

```text
LevelUp.Domain
    ↑
LevelUp.Application
    ↑
LevelUp.Infrastructure
    ↑
LevelUp.Web
```

The actual Web project references Domain, Application, and Infrastructure because it is the composition root. Infrastructure references Application, and Application references Domain. Domain has no project references.

## Documents

- [System overview](overview.md)
- [Solution structure](solution-structure.md)
- [Dependency and ownership rules](dependency-rules.md)
- [Persistence](persistence.md)
- [Authentication and identity](authentication.md)
- [Production hosting](production-hosting.md)

## Architectural priorities

1. Domain invariants remain independent from ASP.NET Core and storage.
2. Application coordinates use cases through commands, queries, validators, handlers, and contracts.
3. Infrastructure implements technical contracts without owning business rules.
4. Web owns HTTP, Blazor state, composition, and visual behavior.
5. Persist authoritative state and derive values that can be calculated consistently.
6. Prefer explicit feature boundaries and shared cross-cutting abstractions over hidden coupling.
