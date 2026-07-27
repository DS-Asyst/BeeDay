# AI Context — LevelUp

## Product

LevelUp is a personal productivity application with RPG-inspired character progression. It is not a game engine. RPG concepts provide motivation and visual progression around productivity workflows.

## Stack

- .NET 10 and C#
- ASP.NET Core
- interactive Blazor Server components
- MediatR
- FluentValidation
- xUnit v3
- bUnit and AngleSharp
- JSON persistence
- IIS on Windows for production hosting
- GitHub Actions for CI and production delivery

## Current product areas

- account registration, authentication, email confirmation, password recovery, and account preferences;
- character creation, onboarding, class/avatar selection, and sidebar progression UI;
- Daily: Habits, recurring Tasks, To-Dos, Projects, ordering, search, and attribute filtering;
- optional activity attributes used only for organization;
- Inventory: Wallets, Transactions, Tags, filters, forms, and responsive states;
- experience curve, automatic idempotent rewards, transaction history, and level-up events;
- shared Blazor Design System and centralized Pixel Icon System;
- JSON persistence, backups, health checks, diagnostics, CI, IIS deployment, and rollback.

## Repository map

```text
src/LevelUp.Domain          business state and invariants
src/LevelUp.Application     use cases, validation, contracts, orchestration
src/LevelUp.Infrastructure  JSON, security, email, caching, auditing, background work
src/LevelUp.Web             Blazor UI, HTTP, composition, diagnostics, assets
tests/                      layer-aligned automated tests
docs/                       maintained source of truth
scripts/                    reviewed local and deployment automation
```

## Product constraints

- Activity attributes are semantic metadata and do not affect XP.
- JSON is the current persistence mechanism; SQL Server is a possible future migration, not current behavior.
- Manual card ordering is authoritative unless the user explicitly chooses another view/sort.
- The Design System and Pixel Icon System are established contracts and should be reused.
- Documentation describes current behavior, not sprint history.
