# AI Context — LevelUp

## Product

LevelUp (a temporary product name) is a personal productivity application with a light gamification layer and personal finance tracking. It is not a game and does not model RPG concepts (characters, classes, equipment, combat). XP and Level exist only to provide light motivation and visible progress around productivity workflows, in the spirit of products like Duolingo rather than Habitica.

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

- account registration with full name, authentication, email confirmation, password recovery, and account preferences;
- profile creation, onboarding, avatar, and sidebar progression UI;
- Daily: Habits, recurring Tasks, To-Dos, Projects, ordering, search, and attribute filtering;
- optional activity attributes used only for organization;
- Wallet: Wallets, Transactions, Wallet Tags, filters, forms, and responsive states;
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
