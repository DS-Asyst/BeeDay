# Contract-First Migration Status (Sprint 14.6, superseded by Sprint 14.7 — see §9)

**Purpose:** the single, precise answer to "how much of the Contract-First migration is actually done
in code today" — verified against the compiled source, not against what any earlier Sprint report
intended or approved for later. Every row below was confirmed by reading the current implementation
during Sprint 13.7, not carried forward from memory of earlier Sprints.

**Do not read this document as a plan.** Plans live in `07-persistence-contracts.md` and the
Sprint 13.4 reports. This document only answers "is it built," as of the state described in §0.

**Read §9 first.** Sprint 14.7 removed the JSON legacy code and `LevelUpData` that §8 still describes
as present (unregistered but compiled). §1–§8 are kept as the historical record of Sprints 13.3–14.6;
none of them describe the code as it exists today.

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

**Sprint 14.5 addendum:** `IUnitOfWork`/`EfUnitOfWork` now coordinate the 8 repositories against one
shared `LevelUpDbContext`, with explicit transaction control (begin/commit/rollback) and standardized
Infrastructure exceptions (`ConcurrencyConflictException`/`PersistenceException` — EF Core's own
exception types no longer leak past Infrastructure). Still does not change anything below — no handler
was migrated, `IUnitOfWork` has zero consumers, JSON remains the only active provider. §5 and §6 are
updated with the new components. See [`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md)
§0.6.

**Contract Completion Step addendum (pre-Sprint 14.6):** the handler migration matrix found 5 contract
gaps (G1–G5) blocking any real handler migration — all closed in the 8 existing contracts/adapters, no
new contract, no `Repository<T>`, Todo still exclusively inside `IProjectRepository`. Still does not
change anything below — no handler migrated, JSON remains the only active provider. §5 updated. See
[`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md) §0.7 and
[`07-persistence-contracts.md`](07-persistence-contracts.md) §14.

**Sprint 14.6 addendum — the migration described as "not complete" throughout this document is now
complete.** SQL Server is the only runtime provider; every row in §1/§2 is migrated; every contract in
§3 has a real consumer; `ILevelUpRepository`/`JsonLevelUpRepository`/`GetLevelUpQuery`/
`GetLevelUpQueryHandler`/`GetLevelUpResponse`/`RequestHandlerBase` are deleted; `FakeLevelUpRepository`
is deleted (replaced by per-Aggregate fakes + `FakeUnitOfWork`, `Application.Tests`). §1, §2, §3, §4, §5
and §7 below are superseded by §8; kept only as the historical record of what Sprints 13.3–14.5 actually
built before anything consumed it. Read §8 first for the current state.

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
`Repository<T>`/`BaseRepository<T>`. Standalone (DI-resolved), each still creates and disposes its own
`LevelUpDbContext` per operation (`IDbContextFactory<LevelUpDbContext>.CreateDbContextAsync()`),
consistent with the Blazor Server rationale in §5.1 — never a shared or long-lived context. All 8 are
registered in DI (`AddScoped`), which only makes them resolvable; **still zero consumers** — no handler
references any of them, and `ILevelUpRepository`/JSON remains the only path actually exercised by the
running application. Since Sprint 14.5, each class also has a second, internal constructor accepting an
externally-owned `LevelUpDbContext` directly — used exclusively by `EfUnitOfWork` (§5.3) to let several
of them share one context; this does not change the standalone behavior described above at all.

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

### 5.3 EF Core Unit of Work (Sprint 14.5) — exists, registered in DI, zero consumers

`IUnitOfWork` (`src/LevelUp.Application/Common/Contracts/IUnitOfWork.cs`) and `EfUnitOfWork`
(`Persistence/SqlServer/EfUnitOfWork.cs`) close the gap flagged in §5.2/`07-persistence-contracts.md`
§6: coordinating writes across more than one of the 8 repositories against a single `LevelUpDbContext`,
with explicit `BeginTransactionAsync`/`CommitTransactionAsync`/`RollbackTransactionAsync`. Registered
`AddTransient` (not `AddScoped`, for the same Blazor Server circuit-lifetime reason as §5.1) — every
resolution creates a fresh instance and a fresh context. **Still zero consumers**: no handler resolves
`IUnitOfWork`; the two cross-Aggregate atomicity boundaries identified in
`07-persistence-contracts.md` §6 (`Habit ↔ User` XP grant, `UserToken ↔ User` token consumption) remain
on JSON, unmigrated.

**Transaction behavior, precisely** (each of the 8 repository write methods still calls
`SaveChangesAsync` internally — accepted, not redesigned): standalone, each call is its own atomic unit,
unchanged from Sprint 14.4. Under `EfUnitOfWork` with an explicit transaction open
(`BeginTransactionAsync`), every one of those internal `SaveChangesAsync` calls, from any repository
resolved off that same `EfUnitOfWork`, participates in that one ambient transaction — none of the 8
repositories need to know a transaction is open. `CommitTransactionAsync` is what makes all of them
durable together; a rollback (explicit or automatic on dispose) reverts every `SaveChangesAsync` that
ran inside that transaction, not just the last one or the one that failed.
`IUnitOfWork.SaveChangesAsync()` itself is for the narrower case of a mutation on an entity still
tracked that no repository operation has persisted yet (e.g. mutating `habit` right after
`Habits.AddAsync(habit)`) — not the normal way work gets persisted. Full account:
[`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md) §0.6.

Standardized, for the first time, how the SQL Server adapters report failure: a new internal
`EfConcurrencySaveChanges` helper (`Persistence/SqlServer/EfConcurrencySaveChanges.cs`) wraps every
`SaveChangesAsync` call made by the 8 repositories and `EfUnitOfWork` — `DbUpdateConcurrencyException`
becomes a new `ConcurrencyConflictException` (`Persistence/Exceptions/`, a `PersistenceException`
subtype alongside the existing JSON ones), any other `DbUpdateException` becomes `PersistenceException`
itself. EF Core's own exception types no longer leak past Infrastructure. Consequence: one Sprint 14.4
test (`EfWalletRepositoryTests.AddAsync_SecondWalletForSameUser_ViolatesUniqueIndex`) now asserts
`PersistenceException` instead of the raw `DbUpdateException` it asserted before — a deliberate,
reported change, not a regression.

`PersistenceContractBoundaryTests.PersistenceContracts_ContainNoGenericRepositoryOrUnapprovedUnitOfWorkAbstraction`
(renamed from Sprint 13.6's version, which asserted no Unit of Work existed yet) now allows exactly
`IUnitOfWork` by name, still rejecting anything else that looks like a second one. Full detail:
[`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md) §0.6.

### 5.4 Contract Completion Step (pre-Sprint 14.6) — G1–G5 closed, zero consumers

The handler migration matrix found 5 contract gaps blocking every real handler migration; all 5 are now
closed in the 8 existing contracts (`Common/Contracts/`) and their `Ef*Repository` adapters — no new
contract, no `Repository<T>`, `Todo` still reachable exclusively through `IProjectRepository`:

- **G1** (persisting a mutation on an already-loaded Aggregate): `UpdateAsync(id, Action<T> mutation,
  ct)` on `IUserRepository`, `IUserTokenRepository`, `IHabitRepository`, `IRecurringTaskRepository`,
  `IProjectRepository`, `IWalletRepository`, `IWalletTagRepository`, `ITransactionRepository` —
  **a corrected shape**, not the disconnected `SaveAsync(entity, ct)` nominally approved in Sprint 13.4
  (`07-persistence-contracts.md` §10); see §14 there for why that shape doesn't hold up against
  `RowVersion` concurrency and `docs/data/02-ef-core-strategy.md` §0.7 for the full technical account.
- **G2**: `IProjectRepository.AddTodoAsync`/`UpdateTodoAsync`/`RemoveTodoAsync`.
- **G3**: `IUserTokenRepository.RevokeActiveAsync` (as originally approved).
- **G4**: `IProjectRepository.MoveTodoAsync` (as originally approved).
- **G5** (found during the matrix, not originally named): `ITransactionRepository.ClearTagReferencesAsync`,
  mirroring `LevelUpData.RemoveWalletTag`'s reference-clearing behavior exactly.

A genuine bug was found and fixed while testing G2's `AddTodoAsync`: touching `context.Entry(todo)`
before any explicit `Add()` attached the new Todo as `Unchanged` instead of `Added` (EF Core's default
for `Entry()` on an untracked entity, vs. `Add()`'s graph-wide cascade) — turned an intended INSERT into
a bogus UPDATE matching zero rows. Fixed with an explicit `context.Todos.Add(todo)` before the shadow
`Position` is set. Full account in `docs/data/02-ef-core-strategy.md` §0.7.

**Still zero consumers**: no handler references any of the new methods; JSON remains the only path
actually exercised. 26 new tests (happy path + a genuine injected-concurrency-conflict test per
`UpdateAsync`, plus `RevokeActiveAsync`/`ClearTagReferencesAsync`/Todo-method coverage), same
`EfLocalDbTestBase` pattern as Sprint 14.4. Full detail:
[`docs/data/02-ef-core-strategy.md`](../data/02-ef-core-strategy.md) §0.7 and
[`07-persistence-contracts.md`](07-persistence-contracts.md) §14.

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

**Sprint 14.5** added `EfUnitOfWorkTests.cs` (6 tests, same `EfLocalDbTestBase`/collection): commit and
rollback across two repositories sharing one `EfUnitOfWork`, dispose-without-commit auto-rollback,
rollback-on-exception proving the earlier successful write is also reverted, `SaveChangesAsync`
persisting a mutation on a still-tracked entity, and a genuine two-context `RowVersion` race resulting
in `ConcurrencyConflictException`.

## 7. What "done" will look like

This migration is complete only when every row in §2 moves to a contract-backed adapter, every row in
§3 gains a real adapter and at least one consumer, `GetLevelUpQuery`/`GetLevelUpResponse` has zero
consumers and is deleted, `ILevelUpRepository` has zero consumers and is deleted, and `FakeLevelUpRepository`
has zero consumers and is deletable. **All of these conditions were met on Sprint 14.6 — see §8.**

## 8. Sprint 14.6 — SQL Server runtime cutover (current state)

Every condition in §7 is satisfied. SQL Server is the only runtime provider; JSON exists only as
unregistered legacy code. Verified by rebuilding the whole solution (`dotnet build -c Release
--warnaserror`, 0 warnings/errors) and running the full suite (780 tests, 0 failures) after the changes
below — not asserted from having made the changes.

### 8.1 Read flows — final state

Both rows from §1 that were "not migrated" now are: `GetCurrentUserQuery` → `CurrentUserResponse`
→ `IUserRepository` backs `Tutorial.razor`/`Account.razor`/`ProfileCreationState`/`Entry.razor`/
`CreateProfile.razor.cs` (5 consumers, not the 3 originally scoped — `Entry.razor`/`CreateProfile.razor.cs`
also called `LevelUpWebService.LoadAsync()`, found during this Sprint). `CurrentUserResponse` grew
`Name`, `Nickname`, `Language`, `Theme`, `HasProfile` beyond its Sprint 13.4 identity-only shape — the
same additive-DTO precedent already used for `DashboardResponse` — because `Account.razor` needed the
profile/preference fields too, not just identity (found during implementation, not in the original plan).
Dashboard and Wallet reads are unchanged from §1 except their backing adapter: `EfDashboardReadService`/
`EfWalletReadService` (`src/LevelUp.Infrastructure/Persistence/SqlServer/`) replace
`JsonDashboardReadService`/`JsonWalletReadService` in DI — same contracts, same handler code, zero
handler-level changes, covered by `EfDashboardReadServiceTests`/`EfWalletReadServiceTests` (`Infrastructure.Tests`,
`EfLocalDbTestBase` pattern).

### 8.2 Write flows — final state

Every handler in §2's table now depends on one or more of the 8 per-Aggregate contracts (single-Aggregate
operations) or `IUnitOfWork` (the cross-Aggregate ones). Confirmed cross-Aggregate boundaries, each
wrapped in `BeginTransactionAsync`/`CommitTransactionAsync` with `try/finally { await
unitOfWork.DisposeAsync(); }` around the whole handler body:

| Boundary | Handler(s) |
|---|---|
| `Habit` + `User` (XP) | `RegisterHabitPositiveCommandHandler` |
| `RecurringTask` + `User` (XP) | `ToggleTaskCommandHandler` |
| `Project`/`Todo` + `User` (XP) | `ToggleTodoCommandHandler` |
| `Project` + `Todo` (cross-Project move) | `UpdateTodoCommandHandler` (only when the destination Project differs from the current one — same-Project edits stay a single, non-transactional `IProjectRepository.UpdateTodoAsync` call) |
| `User` + `UserToken` | `CreateUserCommandHandler`, `CreateAccountCommandHandler`, `ConfirmEmailCommandHandler`, `ResetPasswordCommandHandler` |
| `Wallet` + `Transaction` | `CreateTransactionCommandHandler` (Wallet created on demand), `UpdateTransactionCommandHandler`, `DeleteTransactionCommandHandler` (both touch the Wallet) |
| `WalletTag` + `Transaction` + `Wallet` | `DeleteWalletTagCommandHandler` (clears tag references, then touches the Wallet) |

The last two rows were **found during implementation**, not in the 4 boundaries originally named
(`Habit+XP`, `UserToken+User`, `Project+Todo`, `WalletTag+Transactions`) — flagged for approval before
implementing, per the Sprint's explicit process, not decided silently.

`ReorderActivitiesCommandHandler` (`IHabitRepository`/`IRecurringTaskRepository`/`IProjectRepository`,
no `IUnitOfWork` — reordering is a single-Aggregate write) now validates ownership/duplicates itself
before delegating, because `EfHabitRepository.ReorderAsync`/`EfProjectRepository.ReorderAsync` (Sprint
14.4) silently ignore ids outside the caller's own rows rather than throwing — a behavior gap between
the already-tested repository method and `LevelUpData.ReorderHabits`'s stricter contract, closed in the
handler rather than by changing the approved Sprint 14.4 repository code.

### 8.3 Domain/Application changes required to unblock the cutover

- `User.CompleteProfile(string, string?)` changed from `internal` to `public` — the one Domain file this
  Sprint touched, despite the plan text originally promising none. Every other migrated Aggregate mutation
  method was already public; this one wasn't, because its only prior caller (`LevelUpData.CompleteUserProfile`)
  lived in the same assembly. Flagged and approved before the change; zero behavior difference.
- `CurrentUserGuard.RequireUserId(LevelUpData, ICurrentUserContext)` — the Sprint 13.4-era legacy
  overload, already marked "removed once every handler uses the single-argument overload" — deleted
  once `GetLevelUpQueryHandler` (its last caller) was deleted.
- `IExperienceRewardService.Grant`/`IEmailConfirmationIssuer.Issue` — both took `LevelUpData` before this
  Sprint (a Contract-First violation flagged in `07-persistence-contracts.md` §5.1); now take the already-
  loaded `User` directly, persistence left to the caller.
- A stale Program.cs production guard requiring `LevelUp:Storage:Directory` to be an absolute path was
  removed — it validated a JSON config key nothing reads anymore. `SqlServerOptions.ConnectionString`'s
  own `.Validate(...).ValidateOnStart()` (added this Sprint) already enforces "must be present," in every
  environment, not just non-Development.

### 8.4 DI cutover — final state

`InfrastructureServiceCollectionExtensions.AddLevelUpInfrastructure`: `ILevelUpRepository`/
`JsonLevelUpRepository`, `JsonDashboardReadService`, `JsonWalletReadService`, and every JSON persistence
type with no remaining consumer (`JsonLevelUpDocumentStore`, `JsonStorageGate`, `JsonStorageInitializer`,
`JsonAtomicFileCommitter`, `JsonFileReader`, `JsonFileWriter`, `JsonBackupService`, `JsonStorageHealthCheck`)
are no longer registered. `JsonStoragePaths`/`JsonSerializerOptionsFactory`/`JsonEventJournal` **stay
registered** — `JsonEventJournal` backs `IEventJournal`, a domain-event audit log genuinely unrelated to
the `LevelUpData` persistence cutover (`AuditDomainEventHandler` still depends on it, and still fires on
every published domain event); found and corrected mid-Sprint after the original plan text listed
`JsonEventJournal.cs` for de-registration without accounting for this. `SqlServerHealthCheck` is now
unconditionally registered with `["ready", "storage", "sql"]` — the tags `JsonStorageHealthCheck` used to
own — since SQL Server is no longer optional/local-only; `SqlServerOptions.HealthCheckEnabled` is unused
as a result.

### 8.5 Deleted

`ILevelUpRepository.cs`, `JsonLevelUpRepository.cs`, `GetLevelUpQuery.cs`, `GetLevelUpQueryHandler.cs`,
`GetLevelUpResponse.cs`, `RequestHandlerBase.cs` (Application/Infrastructure), and
`FakeLevelUpRepository.cs` (`Application.Tests`, replaced by `FakeUnitOfWork.cs` — 8 per-Aggregate fakes
sharing one in-memory `LevelUpData` as backing storage, plus `FakeUnitOfWork` itself implementing
`IUnitOfWork` with no-op transaction methods). `LevelUpData.cs`/`LevelUpData.Persistence.cs` (Domain) are
**not** deleted — they remain as what the still-registered JSON legacy pipeline (`JsonLevelUpDocumentStore`
and below) reads/writes, and as the backing store `FakeUnitOfWork` reuses for test convenience.

### 8.6 Test suite — final state

All 5 test projects run against real SQL Server LocalDB (`LevelUp.Infrastructure.Tests` since Sprint
14.4; `LevelUp.Web.Tests`/`LevelUp.E2E.Tests` since this Sprint) — never InMemory/SQLite.
`LevelUpWebApplicationFactory`/`E2EWebApplicationFactory` each generate a uniquely-named LocalDB
connection string per instance, migrate it in `CreateHost`, and `EnsureDeleted` it in `Dispose`; seeding
helpers (`SeedUserAsync`, `FindUserAsync`, `DeactivateUserAsync`, `IssueTokenAsync`) call
`IUserRepository`/`IUserTokenRepository` directly instead of `ILevelUpRepository.UpdateAsync(Action<LevelUpData>)`.
`LevelUp.Infrastructure.csproj` grants `InternalsVisibleTo` to `LevelUp.Web.Tests`/`LevelUp.E2E.Tests` (in
addition to the existing `LevelUp.Infrastructure.Tests` grant) so both factories can name `LevelUpDbContext`
to run migrations/drop the database.

A real, non-obvious bug surfaced only once these factories exercised handlers that resolve `IUnitOfWork`
through a real scope: `EfUnitOfWork` implements only `IAsyncDisposable`, and .NET's DI container throws
(`'EfUnitOfWork' type only implements IAsyncDisposable. Use DisposeAsync to dispose the container.`) if a
scope containing one is disposed *synchronously* — every affected integration test now creates its scope
via `CreateAsyncScope()`/`factory.CreateAuthenticatedScope(...)` (now returning `AsyncServiceScope`) and
disposes it with `await using`, never a plain `using`. Production is unaffected — ASP.NET Core's own
per-request/circuit scope disposal is already asynchronous — but every test helper that resolves `ISender`
inside a manually-created scope had to change.

Application.Tests' 8-per-Aggregate `FakeUnitOfWork` (§8.5) replaced `FakeLevelUpRepository` everywhere;
`PersistenceContractBoundaryTests` anchors its reflection scan on `typeof(IUnitOfWork).Assembly` now
(`ILevelUpRepository` no longer exists to anchor on), and no longer carries the "one deliberate exception"
allowance — every contract is held to "never expose `LevelUpData`" with no exception, since none does.

## 9. Sprint 14.7 — JSON legacy code and `LevelUpData` removed (current state)

Sprint 14.6 (§8) cut the *runtime* over to SQL Server but deliberately left the JSON pipeline and
`LevelUpData` compiled — unregistered, but still in the tree (§8.4, §8.5). Sprint 14.7 answers the
question 14.6 left open: remove that code, not just its DI registration. Verified by rebuilding the
whole solution (`dotnet build -c Release --warnaserror`, 0 warnings/errors), `dotnet format
--verify-no-changes` clean, `dotnet ef migrations has-pending-model-changes` → "No changes have been
made to the model since the last migration," and the full suite (742 tests, 0 failures) after the
changes below.

### 9.1 Deleted — JSON persistence pipeline (Infrastructure)

The entire `src/LevelUp.Infrastructure/Persistence/Json/` folder (14 files: `JsonLevelUpDocumentStore`,
`JsonDashboardReadService`, `JsonWalletReadService`, `JsonStorageGate`, `JsonStorageInitializer`,
`JsonAtomicFileCommitter`, `JsonFileReader`, `JsonFileWriter`, `JsonBackupService`,
`DomainJsonContractResolver`, `JsonSerializerOptionsFactory`, `JsonStoragePaths`,
`LegacyActivityAttributeMigrator`, `LegacyCharacterMigrator`, `LegacyInventoryTagMigrator`),
`Configuration/JsonStorageOptions.cs`, and `HealthChecks/JsonStorageHealthCheck.cs`. None of these had a
production consumer since Sprint 14.6 (§8.4) — this Sprint removes the code itself, closing the risk
that unreachable-but-compiled code invites a second provider to be reintroduced by accident.

### 9.2 Deleted — `LevelUpData` (Domain)

`LevelUpData.cs`/`LevelUpData.Persistence.cs` — the whole-document Aggregate — are deleted, reversing
§8.5's decision to keep them as `FakeUnitOfWork`'s backing store. Every one of its ~55 members was
traced to one of three buckets before deletion, not assumed dead:

- **Already represented by a real Aggregate + Application handler** (`AddHabit`/`AddTask`/`AddProject`
  with explicit `userId`, `Find*`/`Reorder*` with explicit `userId`, `AddUser`, `AddUserToken`,
  `RevokeActiveUserTokens`, `CompleteUserProfile`, `AddWallet`, `AddWalletTag`, `AddTransaction`,
  `Find*`/`RemoveWalletTag`) — every invariant these enforced has a proven replacement: an
  Application-layer explicit check (`IsEmailInUseAsync`/`IsNicknameInUseAsync`/`IsNameInUseAsync`) and/or
  a real SQL Server unique index (`UX_Users_Email`, `UX_Users_Nickname`, `UX_Wallets_User`,
  `UX_WalletTags_User_Name`, `UX_UserTokens_Hash`).
- **Dead schema-migration/bootstrapping code, no SQL equivalent needed** (`EnsureValidState`,
  `ConfirmEmailsForExistingUsers`, `MigrateLegacyProfile`, `EnsureOwnerForLegacyActivities`,
  `EnsureUniqueIds<T>`/`EnsureUniqueValues`/`EnsureUniqueNicknames`/`EnsureUniqueWalletTagNames`,
  `SchemaVersion`, `LegacyProfile`/`LegacyProfileSnapshot`, `LegacyTodos`, `CreateUserSnapshot`, `Todos`
  computed property, `FindUserToken` — zero callers anywhere, dead even before this Sprint) — SQL Server
  starts empty (ADR-002); none of these have meaning outside "a single JSON document."
- **"Ambient current user" convenience, retired as an auth mechanism since Sprint 12.5**
  (`CurrentUserId`/`CurrentUser`/`SetCurrentUser`, the 1-argument `AddHabit`/`AddTask`/`AddProject`/
  `AddTodo` overloads via `AssignCurrentOwner`, the no-`userId` `FindProject`/`FindTodo` overloads) —
  every production handler already receives `userId` explicitly from `CurrentUserGuard`/
  `ICurrentUserContext`; nothing reads these paths.

No item fell into "still-valid business rule with no home" — every invariant's replacement is named
above, not asserted.

### 9.3 `JsonEventJournal` — decoupled, not removed

`IEventJournal.AppendAsync` is write-only (no read-back); its sole consumer is
`AuditDomainEventHandler` (fire-and-forget via `IBackgroundTaskQueue`) — domain-event auditing, not
functional persistence, genuinely independent of the `LevelUpData` cutover. It stays, but no longer
depends on anything deleted in §9.1: `JsonSerializerOptionsFactory`/`JsonStoragePaths` are gone, so
`JsonEventJournal` now takes `(IHostEnvironment, IOptions<EventJournalOptions>)` — a new, minimal
options type (`Directory`, `FileName` only, section `LevelUp:Auditing:EventJournal`) with no field in
common with the deleted `JsonStorageOptions` — and resolves its own rooted-vs-relative journal path
inline instead of calling the deleted `JsonStoragePaths` helper. Its `JsonSerializerOptions` no longer
references `DomainJsonContractResolver` — confirmed unnecessary because domain events (`DomainEvent`
and subtypes, e.g. `UserLeveledUpDomainEvent`) are plain immutable records with public `init`
properties, not entities with private setters, so they serialize correctly with a plain
`JsonSerializerOptions(JsonSerializerDefaults.Web)` + `JsonStringEnumConverter`. Same on-disk format
(`LevelUpEvents.ndjson`), same default/production paths (`appsettings.Production.json`'s
`C:\Apps\LevelUp-Data\Data` preserved verbatim under the new `Auditing:EventJournal` section, not
silently relocated) — `JsonEventJournalTests.cs` rewritten and passing confirms identical append/dedup
behavior under the new configuration source.

### 9.4 `FakeUnitOfWork.cs` (Application.Tests) — redesigned, no replacement document store

§8.5 noted `FakeUnitOfWork`'s 8 fakes shared one in-memory `LevelUpData` as backing storage — itself a
small instance of the pattern this Sprint eliminates elsewhere. Redesigned to 8 independent `List<T>`
properties on `FakeUnitOfWork` (`UsersData`, `UserTokensData`, `HabitsData`, `RecurringTasksData`,
`ProjectsData`, `WalletsData`, `WalletTagsData`, `TransactionsData`), each per-Aggregate fake
constructed with its own list — mirroring how `IUnitOfWork`/`EfUnitOfWork` expose 8 independent
repositories with no aggregating type behind them. No new type groups the 8 lists; deliberately, so no
"document store under a different name" is introduced. All 9 Application.Tests files that read
`repository.Data.*` were rewritten onto the 8 named list properties, using
`Activity.AssignOwner(userId)`/`User.CompleteProfile(nickname, avatar)` directly in place of
`LevelUpData`'s convenience overloads.

### 9.5 Domain.Tests — adjusted for `LevelUpData`'s removal

- `DomainAssemblyBoundaryTests.cs`: reflection anchor changed from `typeof(LevelUpData)` to
  `typeof(User)` — the test itself (Domain assembly never references `System.Text.Json`/EF Core/
  Infrastructure) never depended on which Domain type anchored the `Assembly` lookup.
- `ActivityOrderingTests.cs`, `WalletAggregateRulesTests.cs`: deleted. Every invariant they covered has
  an equivalent already in place: slot-preserving reorder and unknown-id rejection in
  `EfRecurringTaskRepositoryTests`/`EfHabitRepositoryTests` (Sprint 14.4) and
  `FeatureServicesTests.ReorderHandler_ReordersTasks`/`ReorderHandler_RejectsGenuinelyUnknownIdentifier`
  (Application.Tests, §9.6); wallet-tag-uniqueness-at-create, transaction-tag-ownership, and
  delete-tag-preserves-transaction in `WalletHandlersTests.cs` (§9.6).
- `UserIdentityTokenTests.cs`: the 2 `LevelUpData`-specific tests removed
  (`LevelUpData_AddUserToken_RequiresExistingOwner` — covered a check that has no production
  equivalent, already an accepted gap since Sprint 14.6, §3.1/G-series;
  `LevelUpData_RevokeActiveUserTokens_RevokesMatchingTypeOnly` — equivalent already exists,
  `EfUserTokenRepositoryTests.RevokeActiveAsync_RevokesEveryActiveTokenOfThatType`, confirmed present
  before deletion). The other 8 `User`/`UserToken` tests are untouched.
- `UserProfileRulesTests.cs`: the 4 tests that used `LevelUpData` purely as a setup convenience
  (`User_UpdateAvatar_PreservesImmutableNickname`, `CompleteUserProfile_RejectsCompletingProfileTwice`,
  `Profile_ReflectsUnderlyingUserPresentationData`) now call `user.CompleteProfile(nickname, avatar)`
  directly — no behavior change, `LevelUpData.CompleteUserProfile` was a one-line forward to it.
  `CompleteUserProfile_RejectsDuplicateNickname` — a cross-user check that never belonged in a
  single-Aggregate Domain test — was removed from Domain.Tests; confirmed no existing Application.Tests
  coverage for `CompleteUserProfileCommandHandler`'s nickname-collision check, so
  `FeatureServicesTests.CompleteUserProfileHandler_RejectsNicknameAlreadyUsedByAnotherUser` was added
  before the Domain test was removed, not after.
- A stray `tests/LevelUp.Application.Tests/LevelUpDataTests.cs` (2 tests exercising the same dead
  ambient-current-user/bootstrapping paths as §9.2's bucket 2/3 — not part of any Sprint 14.7 inventory
  because it was missed by the original file-by-file audit, found only while re-grepping for
  `LevelUpData` after deletion) was also deleted — no coverage loss, it tested code with zero production
  callers.

### 9.6 Application.Tests — new coverage added, not just moved

Two gaps identified in the pre-implementation audit, closed with new tests before the corresponding
Domain.Tests coverage was removed:

- `FeatureServicesTests.ReorderHandler_RejectsGenuinelyUnknownIdentifier` — locks in that
  `ReorderActivitiesCommandHandler`'s userId-scoped path (the only one production uses) already threw
  `InvalidDomainStateException` for a genuinely unknown id, before this Sprint existed. A closer trace
  during this Sprint's audit found the originally-suspected discrepancy (an approved-plan §6.1 finding
  that this case should return `ArgumentException`/500 like old `LevelUpData.ReorderVisibleItems` did)
  was a false positive: the Domain test that seemed to prove it
  (`ActivityOrderingTests.ReorderRejectsUnknownIdentifier`, deleted in §9.5) called the single-argument,
  ambient-current-user overload no production handler ever used — not the two-argument, userId-scoped
  path production actually calls, which already guaranteed `InvalidDomainStateException` for any
  unowned-or-nonexistent id. **No handler code changed** for this finding; only a regression test was
  added to make the already-correct behavior explicit at the boundary the handler actually exercises.
- `WalletHandlersTests.CreateTag_RejectsDuplicateNameForCurrentUser` — `CreateWalletTagCommandHandler`
  already called `IsNameInUseAsync` before this Sprint; only the *Update* path
  (`UpdateTag_RejectsDuplicateNameForCurrentUser`) had a test. Added to close the gap the plan's §9.4
  flagged before `WalletAggregateRulesTests.cs` (§9.5) was deleted.
- `FeatureServicesTests.CompleteUserProfileHandler_RejectsNicknameAlreadyUsedByAnotherUser` — see §9.5.

### 9.7 `PersistenceContractBoundaryTests.cs` — one guard retired, three kept

`PersistenceContracts_NeverExposeLevelUpDataInAnyMemberSignature` and its `ExposesLevelUpData` helper
were removed — there is no more `LevelUpData` type for any contract to leak. The other three guards are
unchanged: no `System.Text.Json` type in any contract signature, no generic repository/unapproved
second Unit-of-Work abstraction, `LevelUp.Application` never references `LevelUp.Infrastructure`.

### 9.8 What was verified, not changed

No migration was added or modified — `LevelUpData` never had an EF Core mapping, so its removal has no
schema impact; `dotnet ef migrations has-pending-model-changes` confirms the model is exactly what
`InitialCreate` already describes. `AsNoTracking()`/`Include()`/index usage across all `Ef*Repository`/
`EfDashboardReadService`/`EfWalletReadService` files were re-audited and found already correct (10/10
files use `AsNoTracking()`; no query attaches more than one collection `Include`, so `AsSplitQuery()`
would add a round-trip with no cartesian-explosion risk to avoid; every hot-path filter column already
has a covering index) — no code changed as a result, documented here as a completed check rather than
silently omitted. `EfRepositoryBase`/`DbContextLease`/`EfConcurrencySaveChanges` were re-audited for
duplication and found already centralized (Sprint 14.4/14.5); no change.

### 9.9 Known risk, documented (not introduced this Sprint)

Under JSON, `JsonStorageGate` serialized every write — race conditions on uniqueness invariants (email,
nickname, wallet-tag name, one-wallet-per-user) could not occur. Under SQL Server, two concurrent
requests can both pass an `IsXInUseAsync` check before either commits; one then fails the backing unique
index and surfaces as `PersistenceException` (503) instead of the `InvalidDomainStateException` (409)
the explicit check produces under sequential access. This has been true since Sprint 14.1 adopted SQL
Server — not a change introduced by removing `LevelUpData` — but was not previously written down; this
Sprint records it here as a known, accepted property of using a real concurrent-writer database, not a
defect to fix.

### 9.10 Final state

No JSON persistence provider remains. No global document Aggregate remains. No `LevelUpData`. No test
fake backed by `LevelUpData`. No hidden replacement document store — `FakeUnitOfWork` exposes 8
independent lists, matching `EfUnitOfWork`'s 8 independent repositories. `JsonEventJournal` is the only
component with "Json" in its name left in the codebase, and it is audit logging, not persistence. 742
tests pass (93 Domain.Tests, 72 Application.Tests, 120 Infrastructure.Tests, 7 E2E.Tests,
450 Web.Tests), `dotnet build -c Release --warnaserror` and `dotnet format --verify-no-changes` both
clean, no pending EF Core model changes.
