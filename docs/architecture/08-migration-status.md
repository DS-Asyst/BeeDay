# Contract-First Migration Status (Sprint 13.7)

**Purpose:** the single, precise answer to "how much of the Contract-First migration is actually done
in code today" — verified against the compiled source, not against what any earlier Sprint report
intended or approved for later. Every row below was confirmed by reading the current implementation
during Sprint 13.7, not carried forward from memory of earlier Sprints.

**Do not read this document as a plan.** Plans live in `07-persistence-contracts.md` and the
Sprint 13.4 reports. This document only answers "is it built," as of the state described in §0.

## 0. State this document describes

Branch `Contract-First-Architecture-Migration`, working tree as left by Sprint 13.6 (`git status`
unchanged since then), 682/682 tests passing, `dotnet build -c Release --warnaserror` clean. EPIC 13 is
**not complete**. The Application is **not** fully migrated to the Sprint 13.3 persistence contracts.

**Sprint 14.2 addendum:** an EF Core / SQL Server infrastructure foundation now exists (`LevelUpDbContext`,
`DbSet`s, DI, Options, connection string, global conventions).

**Sprint 14.3 addendum:** the EF Core model is now complete (all 10 `IEntityTypeConfiguration<T>`,
Owned/Complex Types, TPC scoped to `Activity`) and an `InitialCreate` migration exists — see
[`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md) §0.2/§0.4. The migration was
verified against a real, disposable SQL Server LocalDB database (`dotnet ef database update` — schema
created without error, all 12 FKs created with the approved `DeleteBehavior`s and no SQL Server error
1785, model confirmed deterministic via `dotnet ef migrations has-pending-model-changes`, disposable
database dropped afterward — see §0.4 for the full account).

**Sprint 14.4 addendum:** all 8 per-Aggregate persistence contracts (§3) now have a concrete EF Core
adapter (`Ef*Repository`), registered in DI. This still does not change anything below — every handler
listed in §1/§2 still depends on `ILevelUpRepository`/JSON, unchanged; the 8 adapters have zero
consumers. §3 and §5 are updated with the new components. See
[`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md) §0.5.

## 1. Read flows

| Flow | Legacy path | Contract-First path | Status |
|---|---|---|---|
| Dashboard (`/daily`) | `GetLevelUpQuery` → `GetLevelUpResponse(LevelUpData)` → `ILevelUpRepository` | `GetDashboardQuery` → `DashboardResponse` → `IDashboardReadService` → `JsonDashboardReadService` | **Migrated.** `DashboardState`, `Home.razor`, `ProfileSidePanel.razor`, `ProjectContextFilter`, `ProjectWorkspace`, `TodoEditorModal`, `DashboardModalState` all consume `DashboardResponse` exclusively. |
| Wallet summary/tags/transaction/transactions-list | `ILevelUpRepository.LoadAsync()` + in-memory filtering inside the handler | `IWalletReadService` → `JsonWalletReadService` | **Migrated.** All 4 query handlers in `WalletQueryHandlers.cs` depend only on `IWalletReadService`. |
| Current-user snapshot for `Tutorial.razor`, `Account.razor`, `ProfileCreationState` | `GetLevelUpQuery` → `GetLevelUpResponse(LevelUpData)` → `ILevelUpRepository` | *(none yet)* | **Not migrated.** These three consumers still call `LevelUpWebService.LoadAsync()`, which still sends `GetLevelUpQuery`. Deliberately excluded from the Dashboard lot (Sprint 13.4) — see `07-persistence-contracts.md` §12 and the Sprint 13.4 Dashboard-migration analysis. |
| `GetCurrentUserQuery` (identity-only response) | `ILevelUpRepository` | *(none yet)* | **Not migrated.** Untouched since before Sprint 13.3. |

`GetLevelUpQuery`/`GetLevelUpResponse`/`GetLevelUpQueryHandler` therefore **still exist in production
code and still have real consumers** — they cannot be removed until the row above them is migrated too
(tracked, not scheduled, as of this Sprint).

## 2. Write flows (commands)

**Zero command handlers have been migrated.** Every one below still depends on `ILevelUpRepository`
and mutates `LevelUpData` through `UpdateAsync(Action<LevelUpData>)`, exactly as before Sprint 13.3:

| Feature | Handlers file | Aggregate(s) touched |
|---|---|---|
| Habits | `HabitCommandHandlers.cs` | `Habit` (+ `User` for XP on RegisterPositive) |
| Tasks | `TaskCommandHandlers.cs` | `RecurringTask` (+ `User` for XP on Toggle) |
| Todos | `TodoCommandHandlers.cs` | `Project`/`Todo` (+ `User` for XP on Toggle) |
| Projects | `ProjectCommandHandlers.cs` | `Project` |
| Ordering | `ReorderActivitiesCommandHandler.cs` | `Habit`/`RecurringTask`/`Project`/`Todo` |
| Wallet commands | `WalletCommandHandlers.cs` | `Wallet`, `Transaction`, `WalletTag` |
| Users | `UserHandlers.cs` | `User` (+ `UserToken` via `IEmailConfirmationIssuer`) |
| Authentication | `AuthenticationHandlers.cs` | `User` |
| Identity | `IdentityHandlers.cs` | `User`, `UserToken` |

Composition root (`RequestHandlerBase.cs`, `InfrastructureServiceCollectionExtensions.cs`) and two Web
files (`Program.cs`'s `OnValidatePrincipal`, `AuthenticatedUserInitializer.cs`) also still depend on
`ILevelUpRepository` directly.

## 3. Persistence contracts by Aggregate (Sprint 13.3, adapters Sprint 14.4)

All 8 exist in `src/LevelUp.Application/Common/Contracts/`, compile cleanly, and are exercised by
`PersistenceContractBoundaryTests` (Sprint 13.6). **As of Sprint 14.4, each has a concrete EF Core
adapter, registered in DI (`AddScoped`) — but still zero production consumers.** Every handler still
depends on `ILevelUpRepository`/JSON, unchanged by this Sprint; registering the 8 adapters makes the
contracts *resolvable* from the container, not *consumed* by anything real. Verified by source search —
zero matches for any of these interface names as a handler constructor dependency, anywhere in `src/`,
except the new `Ef*Repository` adapter itself and its own XML doc comments/cross-links.

| Contract | Aggregate | Adapter | Registered in DI | Consumed by a handler |
|---|---|---|---|---|
| `IUserRepository` | `User` | `EfUserRepository` | Yes | No |
| `IUserTokenRepository` | `UserToken` | `EfUserTokenRepository` | Yes | No |
| `IHabitRepository` | `Habit` | `EfHabitRepository` | Yes | No |
| `IRecurringTaskRepository` | `RecurringTask` | `EfRecurringTaskRepository` | Yes | No |
| `IProjectRepository` | `Project` (+ `Todo`) | `EfProjectRepository` | Yes | No |
| `IWalletRepository` | `Wallet` | `EfWalletRepository` | Yes | No |
| `ITransactionRepository` | `Transaction` | `EfTransactionRepository` | Yes | No |
| `IWalletTagRepository` | `WalletTag` | `EfWalletTagRepository` | Yes | No |

All 8 adapters live in `src/LevelUp.Infrastructure/Persistence/SqlServer/Repositories/` (`internal
sealed`, one concrete class per Aggregate — no generic `Repository<T>`), each creating and disposing its
own `LevelUpDbContext` per operation via `IDbContextFactory<LevelUpDbContext>.CreateDbContextAsync()`
(never a shared/long-lived context). See
[`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md) §0.5 for the write-strategy
decisions (Position assignment on Add, re-fetch-by-Id on Remove/Reorder) and the confirmation that none
of the 8 contracts expose `Update`/`Save` today.

### 3.1 Approved corrections not yet applied to code

`07-persistence-contracts.md` §10 records 4 approved signature additions (`SaveAsync` on
`ITransactionRepository`/`IWalletTagRepository`/`IProjectRepository`, `MoveTodoAsync` on
`IProjectRepository`, `RevokeActiveAsync` on `IUserTokenRepository`). **Verified: none of these methods
exist in the current `.cs` files.** The approval recorded a decision for whichever lot migrates that
handler — it did not, and was never claimed to, change the files at the time.

### 3.2 Atomicity ports — design corrected, never implemented

`07-persistence-contracts.md` §9 documents the corrected (non-callback, `BeginAsync`/`CommitAsync`)
design for `IHabitProgressionTransaction` and `IIdentityTokenTransaction`. **Verified: neither type
exists anywhere in `src/` or `tests/`.** This is a documented design pending implementation, not a
partially-built feature.

## 4. Read services (Sprint 13.3) — adoption status

| Contract | Adapter | Registered in DI | Consumed |
|---|---|---|---|
| `IWalletReadService` | `JsonWalletReadService` | Yes | Yes — 4 query handlers |
| `IDashboardReadService` | `JsonDashboardReadService` | Yes | Yes — `GetDashboardQueryHandler` |

Both fully adopted end-to-end (handler → contract → adapter → real JSON file), each with dedicated
Infrastructure tests against a real temp file (`JsonWalletReadServiceTests.cs`,
`JsonDashboardReadServiceTests.cs`) and Web-level coverage through the real DI container
(`Web.Tests`/`E2E.Tests`).

## 5. Internal Infrastructure component

`JsonLevelUpDocumentStore` — extracted in Sprint 13.4, confirmed still `internal` and still the single
shared read/write/backup pipeline behind `JsonLevelUpRepository` (legacy wrapper),
`JsonWalletReadService`, and `JsonDashboardReadService`. No duplicate JSON I/O pipeline exists anywhere
in `LevelUp.Infrastructure` (reconfirmed in Sprint 13.5's audit, unchanged since).

### 5.1 EF Core foundation (Sprint 14.2) and complete model + migration (Sprint 14.3) — exist, zero consumers

`LevelUpDbContext` (`src/LevelUp.Infrastructure/Persistence/SqlServer/`, `internal`, 10 `DbSet`s) exists,
registered via `AddDbContextFactory<LevelUpDbContext>` — not `AddDbContext` — because `LevelUp.Web` is
Blazor Server and a scoped `DbContext` would live for the whole long-lived circuit; only
`IDbContextFactory<LevelUpDbContext>` is resolvable from the container, never `LevelUpDbContext` itself.
`LevelUpDbContextFactory` (a distinct type, `IDesignTimeDbContextFactory<LevelUpDbContext>` for `dotnet
ef` tooling) also exists. `SqlServerHealthCheck` exists but is registered only when
`SqlServerOptions.HealthCheckEnabled` is `true` — not set in any `appsettings*.json`, so it never runs
today.

**As of Sprint 14.3, the model is complete**: all 10 `IEntityTypeConfiguration<T>`
(`Persistence/SqlServer/Configurations/`) map every column, `CHECK`, index, FK and `DeleteBehavior` from
`01-relational-model.md` exactly; inheritance-mapping strategy is TPC scoped to `Activity` (resolving
what Sprint 14.2 left pending); `UserExperience` is an Owned Type, `ExperienceSource` a Complex Type;
the `InitialCreate` migration exists (`Persistence/SqlServer/Migrations/`), schema-only, no seed data.
**Still zero consumers**: no repository, no read service, no handler references any of this. The
migration itself *has* been verified end-to-end against a real, disposable SQL Server LocalDB database
(applied cleanly, schema/FKs/checks/indexes inspected directly via `sys.*` catalog views, then dropped)
— but that database was throwaway, exists nowhere now, and nothing in the running application talks to
SQL Server.

Sprint 14.3 confirmed a genuine EF Core tooling limitation: no Fluent API or metadata surface can
express an index spanning both an entity's own properties and a nested Complex/Owned Type's properties
— `UX_ExperienceEntries_Dedup` had to be added via raw SQL directly inside the migration's `Up()`/`Down()`,
the standard workaround for this scenario, since empirically verified to create the exact expected
filtered unique index against a real engine. It also confirmed that under TPC, a property inherited from
the hierarchy root can only be configured/ignored once, at the root (`Activity`) — never per derived
type — which forced `Project.Completed` (a fully computed override with no backing field) to be mapped
via `UsePropertyAccessMode(PropertyAccessMode.Field)` instead of being excluded, leaving `Projects` with
a `Completed` column Domain never reads for that type. The multiple-cascade-paths risk (SQL Server error
1785) flagged as residual in the original report is now closed: applying the migration created all 12
FKs, including the two `NO ACTION` and one `SET NULL` overrides, without SQL Server rejecting any of
them. Full detail: [`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md) §0.2/§0.4, and
[`docs/data/01-relational-model.md`](../data/01-relational-model.md) §5.8 and the `Projects`/
`ExperienceEntries` table notes.

### 5.2 EF Core repository adapters (Sprint 14.4) — exist, registered in DI, zero consumers

All 8 `Ef*Repository` classes (`Persistence/SqlServer/Repositories/`) now implement their corresponding
`Common/Contracts` interface exactly — no method was added to any contract, no generic
`Repository<T>`/`BaseRepository<T>`. Each creates and disposes its own `LevelUpDbContext` per operation
(`IDbContextFactory<LevelUpDbContext>.CreateDbContextAsync()`), consistent with the Blazor Server
rationale in §5.1 — never a shared or long-lived context. All 8 are registered in DI (`AddScoped`),
which only makes them resolvable; **still zero consumers** — no handler references any of them, and
`ILevelUpRepository`/JSON remains the only path actually exercised by the running application.

Two design points, not requested by any contract, were necessary to make the adapters work against the
real schema: (1) the `Position` shadow property (Habits/RecurringTasks/Projects/Todos) has no database
default, so `AddAsync` computes the next free ordinal itself, scoped by `UserId` (or `ProjectId` for
Todos); (2) `RemoveAsync`/`ReorderAsync` always re-fetch the target entity by `Id` inside the new
context rather than attaching the instance passed in, because Domain entities never expose the shadow
`RowVersion` concurrency token — attaching a detached instance directly would send a `DELETE`/`UPDATE`
with a default `RowVersion`, guaranteeing a spurious concurrency failure instead of a genuine check.

Confirmed, not assumed: none of the 8 contracts expose `Update`/`Save`/`SaveChanges` today
(`07-persistence-contracts.md` §6/§10/§13 already documented this as a deliberate, still-pending Unit of
Work gap) — Sprint 14.4 implements exactly the methods that exist, adding none. Full detail:
[`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md) §0.5.

## 6. Test suite

Sprint 13.6 consolidated 9 duplicated `ILevelUpRepository` test fakes into `FakeLevelUpRepository`
(+ `FakeCurrentUserContext`, `FakeApplicationCache`), and extended `PersistenceContractBoundaryTests`
with 3 architectural guards (no `System.Text.Json` type in any contract signature, no generic
Repository/UnitOfWork abstraction, `LevelUp.Application` never references `LevelUp.Infrastructure`).
These fakes still back **9 Application.Tests files** exercising the unmigrated handlers listed in §2 —
they remain necessary exactly as long as those handlers do, and are the correct scope for consolidation
(see `07-persistence-contracts.md`'s cross-reference) rather than something to remove now.

**Sprint 14.4** introduced the first tests that run against a real, disposable SQL Server LocalDB
database instead of an in-memory-only model (`Persistence/SqlServer/Repositories/*Tests.cs`,
29 tests): each test class creates a uniquely-named database, applies the real `InitialCreate`
migration, exercises one `Ef*Repository`, then drops the database (`EfLocalDbTestBase`). All 8 classes
share an xunit collection with parallelization disabled, avoiding LocalDB contention — the same pattern
`LevelUp.E2E.Tests` already uses for Playwright/Kestrel.

## 7. What "done" will look like

This migration is complete only when every row in §2 moves to a contract-backed adapter, every row in
§3 gains a real adapter and at least one consumer, `GetLevelUpQuery`/`GetLevelUpResponse` has zero
consumers and is deleted, `ILevelUpRepository` has zero consumers and is deleted, and `FakeLevelUpRepository`
has zero consumers and is deletable. None of these conditions hold today. Sprint 13.8 should audit
against this document's §1–§6 directly rather than re-deriving the inventory from source again.
